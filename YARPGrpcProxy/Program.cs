using k8s;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Steeltoe.Common.Kubernetes;
using Steeltoe.Extensions.Configuration.Kubernetes;
using Yarp.ReverseProxy.Configuration;
using YARPGrpcProxy.bgService;

var builder = WebApplication.CreateBuilder(args);

// Add Steeltoe Kubernetes configuration
builder.Configuration.AddKubernetes();

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
// Add Steeltoe Kubernetes services
builder.Services.AddKubernetesApplicationInstanceInfo();
builder.Services.AddKubernetesClient(k8sClientConfiguration =>
{
    k8sClientConfiguration= KubernetesClientConfiguration.BuildConfigFromConfigFile();
    
});

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
