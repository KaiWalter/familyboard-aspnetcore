﻿using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;

namespace FamilyBoard.Core.Cache
{
    public static class IntegratedTokenCacheExtensions
    {
        /// <summary>Adds an integrated per-user .NET Core distributed based token cache.</summary>
        /// <param name="services">The services collection to add to.</param>
        /// <returns>A <see cref="IServiceCollection"/> to chain.</returns>
        public static IServiceCollection AddIntegratedUserTokenCache(
            this IServiceCollection services
        )
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.AddSingleton<IMsalTokenCacheProvider, IntegratedTokenCacheAdapter>();

            return services;
        }

        /// <summary>Adds an integrated per-user .NET Core distributed based token cache.</summary>
        /// <param name="builder">The Authentication builder to add to.</param>
        /// <returns>A <see cref="AuthenticationBuilder"/> to chain.</returns>
        public static MicrosoftIdentityAppCallsWebApiAuthenticationBuilder AddIntegratedUserTokenCache(
            this MicrosoftIdentityAppCallsWebApiAuthenticationBuilder builder
        )
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Services.AddIntegratedUserTokenCache();

            return builder;
        }
    }
}
