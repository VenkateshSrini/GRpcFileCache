using Cache.Library.CacheWrapper;
using CacheService.Caching;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
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
        //public static IServiceCollection AddCacheProxy(
        //    this IServiceCollection services, IConfiguration configuration)
        //{
        //    services.AddSingleton<ResolverFactory>(new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(25)));

        //    services.AddGrpcClient<CacheServices.CacheServicesClient>(options =>
        //    {
        //        options.Address = new Uri(configuration["CacheService:url"]);


        //    })
        //    .ConfigureChannel(channelOptions =>
        //    {
        //        var methodConfig = new MethodConfig
        //        {
        //            Names = { MethodName.Default },
        //            RetryPolicy = new RetryPolicy
        //            {
        //                MaxAttempts = 5,
        //                InitialBackoff = TimeSpan.FromSeconds(1),
        //                MaxBackoff = TimeSpan.FromSeconds(5),
        //                BackoffMultiplier = 1.5,
        //                RetryableStatusCodes = { Grpc.Core.StatusCode.Unavailable }
        //            }
        //        };
        //        channelOptions.UnsafeUseInsecureChannelCallCredentials = true;
        //        channelOptions.ServiceConfig = new ServiceConfig { 
        //            LoadBalancingConfigs = { new RoundRobinConfig() }, 
        //            MethodConfigs = { methodConfig } 
        //        };
        //    })
        //    .AddCallCredentials((context, metadata) =>
        //    {
        //        var userId = configuration["CacheService:userId"];
        //        var password = configuration["CacheService:password"];
        //        if (!string.IsNullOrEmpty(userId))
        //        {
        //            var credential = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{password}"));
        //            metadata.Add("Authorization", $"Basic {credential}");
        //        }

        //        return Task.CompletedTask;
        //    });
        //    services.AddSingleton<IFileCache, FileCache>();
        //    return services;
        //}
        public static IServiceCollection AddCacheProxy(
            this IServiceCollection services, IConfiguration configuration)
        {
            var userId = configuration["CacheService:userId"];
            var password = configuration["CacheService:password"];
            var uri = new Uri(configuration["CacheService:url"]);
            services.AddGrpcClient<CacheServices.CacheServicesClient>(options =>
            {
                options.Address = uri;
            })
            .ConfigureChannel(channelOptions =>
            {
                var httpsURL = false;
                if ((uri.Scheme == "http") || (uri.Scheme == "https")){
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
            services.AddSingleton<IFileCache, FileCache>();
            return services;
        }
        public static IServiceCollection AddCacheDNSResolver(this IServiceCollection services, 
            IConfiguration configuration)
        {
            services.AddSingleton<ResolverFactory>(new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(25)));
            services.AddScoped<GrpcChannel>(channelService => {
            var methodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };
            return GrpcChannel.ForAddress(configuration.GetValue<string>("CacheService:url"), new GrpcChannelOptions
                {
                    Credentials = ChannelCredentials.Insecure,
                    ServiceConfig = new ServiceConfig { LoadBalancingConfigs = { new RoundRobinConfig() }, MethodConfigs = { methodConfig } },
                    ServiceProvider = channelService
                });
            });
            services.AddScoped<CacheServices.CacheServicesClient>((sp) => {
                var channel = sp.GetRequiredService<GrpcChannel>();
                return new CacheServices.CacheServicesClient(channel);
            });
            services.AddScoped<IFileCache, FileCache>();
            return services;
        }
    }
}

