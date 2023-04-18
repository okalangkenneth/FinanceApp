using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using DinkToPdf;
using DinkToPdf.Contracts;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.Services.IEmailService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Microsoft.AspNetCore.Authentication.Google;


namespace FinanceApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");

                if (!string.IsNullOrEmpty(Configuration["DATABASE_URL"]))
                {
                    var databaseUrl = Configuration["DATABASE_URL"];
                    connectionString = ConvertDatabaseUrlToHerokuConnectionString(databaseUrl);
                }

                options.UseNpgsql(connectionString);
            });



            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options => options.SignIn.RequireConfirmedAccount = false);

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(3);
            });

            services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "keys")));
            services.AddControllersWithViews();

            

            services.AddRazorPages();

            services.AddAuthentication()
                    .AddGoogle(options =>
                    {
            options.ClientId = System.Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            options.ClientSecret = System.Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
                    });


            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
            });

            string keysFolder = Path.Combine(Directory.GetCurrentDirectory(), "keys");
            if (!Directory.Exists(keysFolder))
            {
                Directory.CreateDirectory(keysFolder);
            }

            string sendGridApiKey = Configuration.GetValue<string>("SendGridApiKey");
            services.AddSingleton<IEmailService>(new SendGridEmailService(sendGridApiKey));

            services.AddHttpClient<OpenAIService>();
            services.AddSingleton(x => new OpenAIService(x.GetRequiredService<HttpClient>(), Configuration["OpenAI:ApiKey"], x.GetRequiredService<ILogger<OpenAIService>>()));

            services.AddAuthorization();

            // Add DinkToPdf native library configuration
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
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Startup>();
            var herokuConnectionString = System.Environment.GetEnvironmentVariable("DATABASE_URL");

            if (!string.IsNullOrEmpty(herokuConnectionString))
            {
               

                logger.LogInformation("Heroku Connection String is set.");
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();

                endpoints.MapAreaControllerRoute(
                name: "Identity",
                areaName: "Identity",
                pattern: "Identity/{controller=Account}/{action=Login}/{id?}");
            });

            // Seed dummy data
            // SeedData.Seed(dbContext);
        }
        private string ConvertDatabaseUrlToHerokuConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Database = databaseUri.LocalPath.TrimStart('/'),
                Username = userInfo[0],
                Password = userInfo[1],
                SslMode = Npgsql.SslMode.Require,
                TrustServerCertificate = true,
            };

            return builder.ToString();
        }

    }
}