using DnsClient;
using Yarp.ReverseProxy.Configuration;

namespace YARPGrpcProxy.bgService
{


    public class KubernetesPodResolver : BackgroundService
    {
        private readonly ILogger<KubernetesPodResolver> _logger;
        private readonly IProxyConfigProvider _proxyConfigProvider;
        private readonly string _headlessServiceName;
        private readonly string _namespaceName;
        private readonly int _port;
        private readonly TimeSpan _updateInterval;
        private readonly LookupClient _dnsClient;
        private readonly IConfiguration _configuration;

        public KubernetesPodResolver(ILogger<KubernetesPodResolver> logger,
            IProxyConfigProvider proxyConfigProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _proxyConfigProvider = proxyConfigProvider;
            _headlessServiceName = configuration.GetValue<string>("Kubernetes:HeadlessServiceName");
            _updateInterval = TimeSpan.FromSeconds(30);
            _dnsClient = new LookupClient();
            _configuration = configuration;
            _namespaceName = configuration.GetValue<string>("Kubernetes:Namespace");
            _port = configuration.GetValue<int>("Kubernetes:GrpcPort");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                    var fqdn = $"{_headlessServiceName}.{_namespaceName}.svc.cluster.local";

                    
                    var dnsResult = await _dnsClient.QueryAsync(fqdn, QueryType.A);
                    //foreach (var answer in dnsResult.Answers.ARecords())
                    //{
                    //    _logger.LogInformation($"Found pod IP: {answer.Address}");
                    //}
                    var destinations = dnsResult.Answers.ARecords()
                        .ToDictionary(
                            record => record.Address.ToString(),
                            record => new DestinationConfig { Address = $"http://{record.Address}:{_port}" });

                    
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
