using Yarp.ReverseProxy.Configuration;
using YARPGrpcProxy.bgService;

var builder = WebApplication.CreateBuilder(args);

// Add YARP services
var initialConfig = new InMemoryConfigProvider(
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
        Destinations = new Dictionary<string, DestinationConfig>(),
        LoadBalancingPolicy = "RoundRobin"
    }
]);

builder.Services.AddSingleton<IProxyConfigProvider>(initialConfig);
builder.Services.AddReverseProxy();
// Add background service for pod resolution
builder.Services.AddHostedService<KubernetesPodResolver>();

var app = builder.Build();

app.UseDeveloperExceptionPage();

// Use YARP middleware
app.MapReverseProxy();

app.Run();
