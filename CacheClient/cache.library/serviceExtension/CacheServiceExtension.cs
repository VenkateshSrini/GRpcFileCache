using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using binary.cache.service;
using Grpc.Core;
using System.Threading.Channels;
using cache.library.CacheFacade;
namespace cache.library.serviceExtension
{
    public static class CacheServiceExtension
    {
        public static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
        {
            var userId = configuration.GetValue<string>("cacheService:userId");
            var password = configuration.GetValue<string>("cacheService:password");
            var host = configuration.GetValue<string>("cacheService:host");
            var port = configuration.GetValue<int>("cacheService:port");
            var scheme = configuration.GetValue<string>("cacheService:scheme");
            var uri = new Uri($"{scheme}://{userId}:{password}@{host}:{port}");
            services.AddGrpcClient<Cache.CacheClient>(options =>
                {
                    options.Address = uri;
                })
                .ConfigureChannel(channelOptions => {
                    var httpsURL = false;
                    if ((uri.Scheme == "http") || (uri.Scheme == "https"))
                    {
                        httpsURL = true;
                    }
                    channelOptions.UnsafeUseInsecureChannelCallCredentials = true;
                    if (string.IsNullOrWhiteSpace(userId) && !httpsURL)
                    {
                        channelOptions.Credentials = ChannelCredentials.Insecure;
                    }
                })
                .AddCallCredentials((context, metadata) =>
                {
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var credential = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{password}"));
                        metadata.Add("Authorization", $"Basic {credential}");
                    }
                    return Task.CompletedTask;
                });
            services.AddSingleton<ICacheProvider, CacheProvider>();
            return services;
        }
    }
}
