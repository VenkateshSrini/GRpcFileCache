using DnsClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.Common;
using Steeltoe.Common.Kubernetes;
using Steeltoe.Extensions.Configuration.Kubernetes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace YARPGrpcProxy.bgService
{


    public class KubernetesPodResolver : BackgroundService
    {
        private readonly ILogger<KubernetesPodResolver> _logger;
        private readonly IProxyConfigProvider _proxyConfigProvider;
        private readonly string _headlessServiceName;
        private readonly string _namespaceName;
        private readonly TimeSpan _updateInterval;
        private readonly LookupClient _dnsClient;
        private readonly IConfiguration _configuration;

        public KubernetesPodResolver(ILogger<KubernetesPodResolver> logger,
            IProxyConfigProvider proxyConfigProvider,
            IConfiguration configuration, IApplicationInstanceInfo applicationInstanceInfo)
        {
            _logger = logger;
            _proxyConfigProvider = proxyConfigProvider;
            _headlessServiceName = configuration.GetValue<string>("Kubernetes:HeadlessServiceName");
            _updateInterval = TimeSpan.FromSeconds(30);
            _dnsClient = new LookupClient();
            _configuration = configuration;
            _namespaceName = (applicationInstanceInfo as KubernetesApplicationOptions)?.NameSpace ?? "default";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                    var fqdn = $"{_headlessServiceName}.{_namespaceName}.svc.cluster.local";

                    var dnsResult = await _dnsClient.QueryAsync(fqdn, QueryType.A);
                    var destinations = dnsResult.Answers.ARecords()
                        .ToDictionary(
                            record => record.Address.ToString(),
                            record => new DestinationConfig { Address = $"http://{record.Address}:80" });

                    
                    (_proxyConfigProvider as InMemoryConfigProvider)?.Update(
                        [
                            new RouteConfig
                            {
                                RouteId = "grpc_route",
                                ClusterId = "grpc_cluster",
                                Match = new RouteMatch
                                {
                                    Path = "/{**catch-all}"
                                }
                            }
                        ], 
                        [
                            new ClusterConfig
                                {
                                    ClusterId = "grpc_cluster",
                                    Destinations = destinations,
                                    LoadBalancingPolicy = "RoundRobin"
                                }
                        ]
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating pod IPs");
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }
        }
    }

}
