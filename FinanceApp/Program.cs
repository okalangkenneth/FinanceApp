using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services.IEmailService;
using FinanceApp.Services.SpendingAnalysis;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

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
builder.Services.AddSingleton<IEmailService>(new SendGridEmailService(sendGridApiKey));

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

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
