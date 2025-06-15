using System;
using System.IO;
using FamilyBoard.Application.Utils;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace FamilyBoard.Core;

public static class StartupExtensions
{
    public static IServiceCollection AddTokenCache(this IServiceCollection services)
    {
        var tokenKeyCachePath =
            System.Environment.GetEnvironmentVariable("TOKENKEYCACHEPATH") ?? ".";

        services
            .AddDataProtection()
            // This helps surviving a restart: a same app will find back its keys. Just ensure to create the folder.
            .PersistKeysToFileSystem(new DirectoryInfo(tokenKeyCachePath))
            // This helps surviving a site update: each app has its own store, building the site creates a new app
            .SetApplicationName("FamilyBoard")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

        services.AddDiskCache(options =>
        {
            options.ActivitiesPath = Path.Combine(
                tokenKeyCachePath,
                "msalAccountActivityStore.json"
            );
            options.CachePath = Path.Combine(tokenKeyCachePath, "accessTokens.json");
        });

        return services;
    }

    public static IServiceCollection AddCookieConfiguration(this IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
            options.HandleSameSiteCookieCompatibility();
        });

        return services;
    }

    public static IServiceCollection AddUIAndApiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string[] initialScopes = configuration.GetValue<string>(Constants.GraphScope)?.Split(' ');

        // Sign-in users with the Microsoft identity platform
        // Configures the web app to call a web api (Ms Graph)
        // Sets the IMsalTokenCacheProvider to be the IntegratedTokenCacheAdapter
        services
            .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(configuration, Constants.AzureAdConfigSectionName)
            .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(configuration.GetSection(Constants.GraphConfigSectionName))
            .AddIntegratedUserTokenCache();

        services
            .AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
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
            options.ViewLocationFormats.Add(
                "/Application/Views/{1}/{0}" + RazorViewEngine.ViewExtension
            );
            options.ViewLocationFormats.Add(
                "/Application/Views/Shared/{0}" + RazorViewEngine.ViewExtension
            );
        });

        return services;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddTransient<IGraphService, GraphService>();

        // Add all calendar services - sequence listed here reflects sequence calendar types are displayed on calendar within a day
        services.AddTransient<ICalendarService, SchoolHolidaysService>();
        services.AddTransient<ICalendarService, PublicHolidaysService>();
        services.AddTransient<ICalendarService, OutlookService>();

        // Add only one image service
        services.AddTransient<IImageService, OnedriveService>();

        return services;
    }
}
