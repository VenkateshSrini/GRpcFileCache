using Microsoft.OpenApi.Models;
using Net.DistributedFileStoreCache;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(webOptions =>
{

    webOptions.ListenAnyIP(5275, kestrelOptions =>
    {
        kestrelOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
    webOptions.ListenAnyIP(7007, kestrelOptions =>
    {
        kestrelOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc()
                .AddJsonTranscoding();
builder.Services.AddDistributedFileStoreCache(options =>
{
    if (!Directory.Exists("cacheDir"))
        Directory.CreateDirectory("cacheDir");
    options.PathToCacheFileDirectory = "cacheDir";
    options.WhichVersion = FileStoreCacheVersions.String;
    options.FirstPartOfCacheFileName = "cacheStore";
}, builder.Environment);
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo { Title = "File based Super fast caching", Version = "v1" });
    var filePath = Path.Combine(AppContext.BaseDirectory, "CacheService.xml");
    c.IncludeXmlComments(filePath);
    c.IncludeGrpcXmlComments(filePath, includeControllerXmlComments: true);
});
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cache Service V1");
});
// Configure the HTTP request pipeline.
app.MapGrpcService<CacheService.Services.CacheService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
