using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using binary.cache.service.domain;
using binary.cache.service.Utils;
using Microsoft.Extensions.Hosting;

public class FileCleanupService : BackgroundService
{
    //private readonly TimeSpan _fileAgeLimit = TimeSpan.FromHours(1);
    private readonly string _cacheMetadata;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(15);
    private readonly string _directoryPath = "YourDirectoryPathHere";
    private readonly ILogger<FileCleanupService> _logger;

    public FileCleanupService(ILogger<FileCleanupService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _cleanupInterval = TimeSpan.FromMinutes(configuration.GetValue<int>("CleanUpScanIntervalInMinutes"));
        _directoryPath = configuration.GetValue<string>("CachePath");
        _cacheMetadata = configuration.GetValue<string>("CacheMetadata");
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (Directory.Exists(_directoryPath))
                DeleteOldFiles(_directoryPath);

            await Task.Delay(_cleanupInterval, stoppingToken);
        }
    }

    private void DeleteOldFiles(string directoryPath)
    {
        var directoryInfo = new DirectoryInfo(directoryPath);

        foreach (var file in directoryInfo.GetFiles(_cacheMetadata, SearchOption.AllDirectories))
        {
            var cacheMetadata = GetCacheMetadata(file);
            if ((DateTime.Now - file.LastAccessTime).TotalSeconds 
                             > cacheMetadata.TimeToLiveInSeconds) 
            {
                try
                {
                    _logger.LogDebug($"Deleting file: {file.FullName} as time to live exceeded");
                    file.Delete();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting file: {file.FullName} Message:- {ex.Message}" +
                        $"\n Stack Trace:- {ex.StackTrace}");
                }
            }
        }
    }
    private CacheMetadata GetCacheMetadata(FileInfo file)
    {
        
        using var fileStream = file.OpenRead();
        return ProtobufSerializationHeper.Deserialize<CacheMetadata>(fileStream);
    }
}
