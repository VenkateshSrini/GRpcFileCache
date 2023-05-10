using Cache.Library.CacheWrapper;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cache.Library.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Grpc.Net.ClientFactory;

namespace Cache.Library.ServiceExtensions
{
    public static class CacheServiceExtension
    {
        public static IServiceCollection AddCacheProxy(
            this IServiceCollection services, IConfiguration configuration)
        {
            var userId = configuration["CacheService:userId"];
            var password = configuration["CacheService:password"];
            var uri = new Uri(configuration["CacheService:url"]);
            services.AddHttpContextAccessor();
            services.AddSingleton<LoggingInterceptor>();
            if (uri.Scheme.ToLower()=="dns")
                services.AddSingleton<ResolverFactory>(new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(25)));
            services.AddGrpcClient<CacheServices.CacheServicesClient>((serviceProvider,options)=>
            {
                options.Address = uri;
                var httpAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var logger = serviceProvider.GetRequiredService<ILogger<LoggingInterceptor>>();
                //var registration = new Grpc.Net.ClientFactory.InterceptorRegistration(InterceptorScope.Channel,
                //    )
            })
            .AddInterceptor<LoggingInterceptor>()
            .ConfigureChannel((serviceProvider,channelOptions) =>
            {
                var httpsURL = false;
                if ((uri.Scheme == "http") || (uri.Scheme == "https"))
                {
                    httpsURL = true;
                }
                channelOptions.UnsafeUseInsecureChannelCallCredentials = true;
                if (string.IsNullOrWhiteSpace(userId) && !httpsURL)
                {
                    
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
                    channelOptions.Credentials = ChannelCredentials.Insecure;
                    channelOptions.ServiceConfig = new ServiceConfig { LoadBalancingConfigs = { new RoundRobinConfig() }, MethodConfigs = { methodConfig } };
                    channelOptions.ServiceProvider = serviceProvider;
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

