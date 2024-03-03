namespace binary.cache.service.LRUCache
{
    public class FolderWatcherService
    {
        private readonly ILogger<FolderWatcherService> _logger;
        public FolderWatcherService(ILogger<FolderWatcherService> logger)
        {
            _logger = logger;
        }
        public void WatchFolder(string path, LRUCache<byte[]> cache)
        {
            var watcher = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.bin"
            };

            watcher.Changed += (source, e) =>
            {
                // Update the cache when a file is changed
                var fileInfo = new FileInfo(e.FullPath);
                var subkey = fileInfo.Directory?.Name;
                var key = fileInfo.Directory?.Parent?.Name;
                var bytes = File.ReadAllBytes(e.FullPath);
                var sizeInKB = bytes.Length/1024;
                var sizeInMB = sizeInKB/1024;
                _logger.LogInformation($"File {e.FullPath} has been changed, updating cache");
                cache.Add(key, subkey, bytes, sizeInMB);
                
            };

            watcher.EnableRaisingEvents = true;
        }
    }
}
