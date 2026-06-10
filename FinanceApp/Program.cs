using DinkToPdf;
using DinkToPdf.Contracts;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.Services.IEmailService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

var builder = WebApplication.CreateBuilder(args);

// Heroku-era PORT contract preserved as-is; port consolidation is Phase 3
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "10000"));
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("HerokuConnection"));
    }
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

builder.Services.AddHttpClient<OpenAIService>();
builder.Services.AddSingleton(x => new OpenAIService(x.GetRequiredService<HttpClient>(), builder.Configuration["OpenAI:ApiKey"], x.GetRequiredService<ILogger<OpenAIService>>()));

builder.Services.AddAuthorization();

// DinkToPdf native library configuration (DinkToPdf is replaced in Phase 1D)
AssemblyLoadContext.Default.ResolvingUnmanagedDll += (assembly, libraryName) =>
{
    if (libraryName.StartsWith("libwkhtmltox"))
    {
        string arch = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
        string libPath = Path.Combine(Directory.GetCurrentDirectory(), "wkhtmltox", arch, "libwkhtmltox");
        return NativeLibrary.Load(libPath);
    }
    return IntPtr.Zero;
};
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

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
