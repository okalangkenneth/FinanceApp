using FinanceApp.Configuration;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services.IEmailService;
using FinanceApp.Services.SpendingAnalysis;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Serilog (portfolio convention): structured console logging, lean setup —
// no file/ELK sinks here. Replaces the default Microsoft logging providers.
builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// QuestPDF Community license: free for individuals and for organizations with
// less than $1M USD annual gross revenue (https://www.questpdf.com/license/).
// This is a personal portfolio project, which falls within the Community tier.
QuestPDF.Settings.License = LicenseType.Community;

// Listen port: single source of truth is the standard ASPNETCORE_HTTP_PORTS /
// ASPNETCORE_URLS contract (audit item 34). The aspnet:8.0 image defaults to
// 8080 (compose maps 8888:8080); host dev runs on 8888 via launchSettings.
// The Heroku-era PORT env var + ListenAnyIP override is gone.

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

// Options pattern with validation at startup. Both API keys stay optional
// (the app degrades gracefully without them — Phase 2 decision for SendGrid,
// clear runtime error for Anthropic), but inconsistent config fails fast.
builder.Services.AddOptions<SendGridOptions>()
    .Bind(builder.Configuration.GetSection(SendGridOptions.SectionName))
    .Validate(o => string.IsNullOrEmpty(o.ApiKey) || !string.IsNullOrEmpty(o.SenderEmail),
        "SendGrid:SenderEmail is required when SendGrid:ApiKey is set.")
    .ValidateOnStart();

builder.Services.AddOptions<AnthropicOptions>()
    .Bind(builder.Configuration.GetSection(AnthropicOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IEmailService, SendGridEmailService>();

// Typed client via IHttpClientFactory (replaces the old AddHttpClient +
// singleton-capturing-HttpClient double registration). Standard resilience
// handler supplies retry/backoff for 429/5xx, honoring Retry-After.
builder.Services.AddHttpClient<ISpendingAnalysisService, AnthropicSpendingAnalysisService>((sp, client) =>
{
    var anthropicOptions = sp.GetRequiredService<IOptions<AnthropicOptions>>().Value;
    client.BaseAddress = new Uri("https://api.anthropic.com");
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    if (!string.IsNullOrEmpty(anthropicOptions.ApiKey))
    {
        client.DefaultRequestHeaders.Add("x-api-key", anthropicOptions.ApiKey);
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

// Demo data for the dev stack and the Phase 6 recorded demo (audit item 17):
// compose sets SEED_DEMO_DATA=true; the demo password comes from
// SeedDemo:Password (gitignored .env), never from code.
if (string.Equals(Environment.GetEnvironmentVariable("SEED_DEMO_DATA"), "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    await SeedData.SeedDemoAsync(scope.ServiceProvider, app.Logger);
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

// After UseStaticFiles so asset requests don't flood the request log
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
// After UseRouting (required for [EnableRateLimiting] endpoint policies) and
// after UseAuthentication so the per-user partition key sees the identity.
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHealthChecks("/health");

app.Run();
