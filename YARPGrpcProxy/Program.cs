using Microsoft.OpenApi.Models;

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

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "YARP gRPC Proxy", Version = "v1" });
});
var executionMode = builder.Configuration.GetValue<string>("RunIn");



// Add background service for pod resolution
builder.Services.AddHostedService<KubernetesPodResolver>();

var app = builder.Build();

// Use Swagger middleware
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "YARP gRPC Proxy v1"));


// Use YARP middleware
app.MapReverseProxy();

app.Run();
