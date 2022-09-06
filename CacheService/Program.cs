using Net.DistributedFileStoreCache;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(webOptions =>
{
    //webOptions.ListenLocalhost(5275, kestrelOptions => {
    //    kestrelOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    //});
    webOptions.ListenAnyIP(5275, kestrelOptions =>
    {
        kestrelOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDistributedFileStoreCache(options =>
{
    if (!Directory.Exists("cacheDir"))
        Directory.CreateDirectory("cacheDir");
    options.PathToCacheFileDirectory = "cacheDir";
    options.WhichVersion = FileStoreCacheVersions.String;
    options.FirstPartOfCacheFileName = "cacheStore";
}, builder.Environment);
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CacheService.Services.CacheService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
