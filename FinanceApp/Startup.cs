using DinkToPdf;
using DinkToPdf.Contracts;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services;
using FinanceApp.Services.IEmailService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace FinanceApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString(env.IsProduction() ? "HerokuConnection" : "DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options => options.SignIn.RequireConfirmedAccount = false);

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(3);
            });

            services.AddControllersWithViews();

            services.AddRazorPages();

            services.Configure<CookieAuthenticationOptions>(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
            });

            string sendGridApiKey = Configuration["SendGrid:ApiKey"];
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext)
        {
            if (env.IsDevelopment())
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
           SeedData.Seed(dbContext);
        }

    }
}
