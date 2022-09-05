using CacheClient;
using CacheServiceWebProxy.Cache;
using Grpc.Core;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpcClient<Cache.CacheClient>(options =>
{
    options.Address = new Uri(builder.Configuration["CacheService:url"]);
})
.ConfigureChannel(channelOptions =>
{
    channelOptions.UnsafeUseInsecureChannelCallCredentials = true;
})
.AddCallCredentials((context,metadata)=>
{
    var userId = builder.Configuration["CacheService:userId"];
    var password = builder.Configuration["CacheService:password"];
    if (!string.IsNullOrEmpty(userId))
    {
        var credential = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{password}"));
        metadata.Add("Authorization", $"Basic {credential}");
    }
    return Task.CompletedTask;
});
builder.Services.AddSingleton<IFileCache, FileCache>();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();

app.MapControllers();

app.Run();
