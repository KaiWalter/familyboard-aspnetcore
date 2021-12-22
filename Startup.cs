using FamilyBoard.Application.Utils;
using FamilyBoard.Core.Cache;
using FamilyBoard.Core.Calendar;
using FamilyBoard.Core.Graph;
using FamilyBoard.Core.Image;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;
using System.IO;

using Constants = FamilyBoard.Core.Constants;

namespace FamilyBoard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var tokenKeyCachePath = System.Environment.GetEnvironmentVariable("TOKENKEYCACHEPATH") ?? ".";

            services.AddDataProtection()
                    // This helps surviving a restart: a same app will find back its keys. Just ensure to create the folder.
                    .PersistKeysToFileSystem(new DirectoryInfo(tokenKeyCachePath))
                    // This helps surviving a site update: each app has its own store, building the site creates a new app
                    .SetApplicationName("FamilyBoard")
                    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

            services.AddDiskCache(options =>
            {
                options.ActivitiesPath = Path.Combine(tokenKeyCachePath, "msalAccountActivityStore.json");
                options.CachePath = Path.Combine(tokenKeyCachePath, "accessTokens.json");
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            services.AddSwaggerGen();

            string[] initialScopes = Configuration.GetValue<string>(Constants.GraphScope)?.Split(' ');

            // Sign-in users with the Microsoft identity platform
            // Configures the web app to call a web api (Ms Graph)
            // Sets the IMsalTokenCacheProvider to be the IntegratedTokenCacheAdapter
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration, Constants.AzureAdConfigSectionName)
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                .AddMicrosoftGraph(Configuration.GetSection(Constants.GraphConfigSectionName))
                .AddIntegratedUserTokenCache();

            services.AddControllersWithViews(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                });


            services.AddRazorPages();
            services.Configure<RazorViewEngineOptions>(options =>
                {
                    options.ViewLocationFormats.Clear();
                    options.ViewLocationFormats.Add("/Application/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
                    options.ViewLocationFormats.Add("/Application/Views/Shared/{0}" + RazorViewEngine.ViewExtension);
                });


            services.AddTransient<IGraphService, GraphService>();

            // Add all calendar services - sequence listed here reflects sequence calendar types are displayed on calendar within a day
            services.AddTransient<ICalendarService, SchoolHolidaysService>();
            services.AddTransient<ICalendarService, PublicHolidaysService>();
            services.AddTransient<ICalendarService, OutlookService>();

            // Add only one image service
            services.AddTransient<IImageService, OnedriveService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseStaticFiles();

            app.UseCookiePolicy();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
