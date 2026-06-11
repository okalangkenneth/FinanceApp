using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services.IEmailService;
using FinanceApp.Services.SpendingAnalysis;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF Community license: free for individuals and for organizations with
// less than $1M USD annual gross revenue (https://www.questpdf.com/license/).
// This is a personal portfolio project, which falls within the Community tier.
QuestPDF.Settings.License = LicenseType.Community;

// Heroku-era PORT contract preserved as-is; port consolidation is Phase 3
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "10000"));
});

// PostgreSQL everywhere — dev and prod use the same provider so one
// migration history serves both (the old SQL Server dev branch had none)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Single intentional value: email confirmation is required (the email flow
// exists). The old false-override existed only to work around the
// ApplicationUser.EmailConfirmed shadow property, removed in this commit.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(3);
});

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// The old Startup configured unnamed CookieAuthenticationOptions, which the
// Identity application cookie (a named scheme) never read — these settings
// were silently inert. ConfigureApplicationCookie makes them take effect.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
});

string sendGridApiKey = builder.Configuration["SendGrid:ApiKey"];
builder.Services.AddSingleton<IEmailService>(sp =>
    new SendGridEmailService(sendGridApiKey, sp.GetRequiredService<ILogger<SendGridEmailService>>()));

// Typed client via IHttpClientFactory (replaces the old AddHttpClient +
// singleton-capturing-HttpClient double registration). Standard resilience
// handler supplies retry/backoff for 429/5xx, honoring Retry-After.
builder.Services.AddHttpClient<ISpendingAnalysisService, AnthropicSpendingAnalysisService>(client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com");
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    string anthropicApiKey = builder.Configuration["Anthropic:ApiKey"];
    if (!string.IsNullOrEmpty(anthropicApiKey))
    {
        client.DefaultRequestHeaders.Add("x-api-key", anthropicApiKey);
    }
}).AddStandardResilienceHandler();

builder.Services.AddAuthorization();

// The spending-analysis endpoints call a metered third-party API (Anthropic)
// from a button click — a tight per-user window caps the spend if the button
// is hammered or scripted. Applied via [EnableRateLimiting("ai-analysis")].
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("ai-analysis", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Endpoints are [Authorize]d, so the identity name is always set;
            // the fallback partition only ever sees unauthenticated 401 traffic.
            partitionKey: httpContext.User.Identity?.Name ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// Liveness + database reachability for compose/Kubernetes probes
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();

// Dev-stack convenience: docker-compose sets RUN_MIGRATIONS=true so the schema
// is created/updated on startup. Production runs migrations as a dedicated
// job before rollout (wired in Phases 4/5), never in-process.
if (string.Equals(Environment.GetEnvironmentVariable("RUN_MIGRATIONS"), "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var pending = db.Database.GetPendingMigrations().ToList();
    app.Logger.LogInformation("[Program] RUN_MIGRATIONS=true, applying {Count} pending migration(s): {Migrations}",
        pending.Count, string.Join(", ", pending));
    db.Database.Migrate();
    app.Logger.LogInformation("[Program] Migrations applied");
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Only outside Development: the local docker-compose stack serves plain HTTP
// (no TLS endpoint), and redirecting with no known https port just logs a
// warning on every cold start. TLS termination is a Phase 4 concern.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
// After UseRouting (required for [EnableRateLimiting] endpoint policies) and
// after UseAuthentication so the per-user partition key sees the identity.
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHealthChecks("/health");

app.Run();
