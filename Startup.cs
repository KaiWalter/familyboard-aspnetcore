using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using FamilyBoard.Application.Utils;
using FamilyBoard.Core.Calendar;
using FamilyBoard.Core.Cache;
using FamilyBoard.Core.Image;

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
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });

            string[] initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                .AddDistributedTokenCaches();

            services.AddDiskCache(options =>
            {
                options.CachePath = System.Environment.GetEnvironmentVariable("TOKENCACHE") ?? ".tokencache";
            });

            services.AddControllersWithViews(options =>
                    {
                        var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();
                        options.Filters.Add(new AuthorizeFilter(policy));
                    })
                    .AddMicrosoftIdentityUI()
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

            // Add the UI support to handle claims challenges
            services.AddServerSideBlazor()
               .AddMicrosoftIdentityConsentHandler();

            // to do https://github.com/ligershark/WebOptimizer

            // Add all calendar services - sequence listed here reflects sequence calendar types are displayed on calendar within a day
            services.AddTransient<ICalendarService, SchoolHolidaysService>();
            services.AddTransient<ICalendarService, PublicHolidaysService>();
            services.AddTransient<ICalendarService, OutlookService>();

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
            });
        }
    }
}
