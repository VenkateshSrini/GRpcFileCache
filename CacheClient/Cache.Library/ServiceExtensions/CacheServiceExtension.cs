using Cache.Library.CacheWrapper;
using CacheService.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache.Library.ServiceExtensions
{
    public static class CacheServiceExtension
    {
        public static IServiceCollection AddCacheProxy(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGrpcClient<CacheServices.CacheServicesClient>(options =>
            {
                options.Address = new Uri(configuration["CacheService:url"]);
            })
            .ConfigureChannel(channelOptions =>
            {
                channelOptions.UnsafeUseInsecureChannelCallCredentials = true;
            })
            .AddCallCredentials((context, metadata) =>
            {
                var userId = configuration["CacheService:userId"];
                var password = configuration["CacheService:password"];
                if (!string.IsNullOrEmpty(userId))
                {
                    var credential = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{password}"));
                    metadata.Add("Authorization", $"Basic {credential}");
                }
                return Task.CompletedTask;
            });
            services.AddSingleton<IFileCache, FileCache>();
            return services;
        }
    }
}

