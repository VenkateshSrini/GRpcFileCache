using binary.cache.service;
using binary.cache.service.domain;
using binary.cache.service.LRUCache;
using binary.cache.service.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(webOptions =>
{
    var gRpcPort = builder.Configuration.GetValue<int>("http2Port");
    var httpPort = builder.Configuration.GetValue<int>("http1Port");
    webOptions.ListenAnyIP(gRpcPort, kestrelOptions =>
    {
        kestrelOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
    webOptions.ListenAnyIP(httpPort, kestrelOptions =>
    {
        kestrelOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});
// Add services to the container.
builder.Services.AddGrpc()
                .AddJsonTranscoding();
builder.Services.AddScoped<ICacheManagement, CacheManagement>();
builder.Services.AddSingleton<LRUCache<byte[]>>();
builder.Services.AddSingleton<FolderWatcherService>();
builder.Services.AddHostedService<FileCleanupService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<CacheService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
