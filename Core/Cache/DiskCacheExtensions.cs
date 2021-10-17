using System;
using IntegratedCacheUtils.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FamilyBoard.Core.Cache
{
    public static class DiskCacheExtensions
    {
        public static IServiceCollection AddDiskCache(this IServiceCollection services, Action<DiskCacheOptions> configureAction)
        {
            var options = new DiskCacheOptions();
            configureAction(options);

            services.AddSingleton(Options.Create(options));
            services.AddScoped<IMsalAccountActivityStore, FileSystemMsalAccountActivityStore>();
            services.AddSingleton<IDistributedCache, DiskCacheHandler>();
            return services;
        }
    }

}