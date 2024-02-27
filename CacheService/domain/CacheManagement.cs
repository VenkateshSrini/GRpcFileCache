using binary.cache.service.Utils;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Polly;
using System.IO;
using System.IO.Compression;
namespace binary.cache.service.domain
{
    public class CacheManagement : ICacheManagement
    {
        private readonly string _cachePath = "cacheDir";
        private readonly string _cacheExtension = ".cache";
        private readonly string _cacheFile = "data";
        private readonly string _cacheMetadata = "metadata.bin";
        private readonly ILogger<CacheManagement> _logger;
        private readonly IConfiguration _configuration;
        public CacheManagement(ILogger<CacheManagement> logger, IConfiguration configuration)
        {
            _logger=logger;
            _configuration=configuration;
            if(!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }
        }
        public byte[] Get(string key, string subKey)
        {
            if (!Directory.Exists($"{_cachePath}"))
            {
                return Array.Empty<byte>();
            }
            if (!Directory.Exists($"{_cachePath}/{key}"))
            {
                return Array.Empty<byte>();
            }
            if (File.Exists($"{_cachePath}/{key}/{subKey}/{_cacheFile}.{_cacheExtension}"))
            {
                return GetCacheValues($"{_cachePath}/{key}/{subKey}");
            }
            return Array.Empty<byte>();
        }
        private byte[] GetCacheValues(string path)
        {
            var cacheMetadata = GetCacheMetadata(path);
            
            if ( DateTime.Now.Subtract(cacheMetadata.LastAccessed).Seconds > cacheMetadata.TimeToLiveInSeconds)
            {
                Directory.Delete(path, true);
                return Array.Empty<byte>();
            }
            cacheMetadata.LastAccessed = DateTime.Now;
            cacheMetadata.Path = $"{path}/{_cacheFile}.{_cacheExtension}";
            SetCacheMetadata(path, cacheMetadata);
            return File.ReadAllBytes($"{path}/{_cacheFile}.{_cacheExtension}");

        }
        private CacheMetadata GetCacheMetadata(string path)
        {
            if (!File.Exists($"{path}/{_cacheMetadata}"))
            {
                return new CacheMetadata { 
                    LastAccessed = DateTime.Now,
                    TimeToLiveInSeconds=_configuration.GetValue<int>("TimePeriodinMinutes")*60,
                    KeyCounter=0,
                    Path=path
                };
            }
            using var fileStream = new FileStream($"{path}/{_cacheMetadata}", FileMode.Open);
            return ProtobufSerializationHeper.Deserialize<CacheMetadata>(fileStream);
        }
        private void SetCacheMetadata(string path, CacheMetadata cacheMetadata)
        {
           
            using var memoryStream = ProtobufSerializationHeper.Serialize(cacheMetadata);
            if (!Trywrite($"{path}/{_cacheMetadata}", 3, memoryStream)) throw new Exception("Error in writing metadata");
           
        }
        private bool Trywrite(string filePath, int retryCount, MemoryStream memoryStream)
        {
            var policy = Policy
                .Handle<IOException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(3));
            
            bool result = false;

            policy.Execute(() =>
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // File is not locked
                    memoryStream.Position = 0L;
                    memoryStream.CopyTo(fileStream);
                    fileStream.Flush();
                    memoryStream.Close();
                    fileStream.Close();
                    result = true;
                }
            });

            return result;
        }
        public long GetSubkeyCount(string key)
        {
            if (!Directory.Exists($"{_cachePath}"))
            {
                return 0;
            }
            if (!Directory.Exists($"{_cachePath}/{key}"))
            {
                return 0;
            }
            var directoryinfo = new DirectoryInfo($"{_cachePath}/{key}");
            return directoryinfo.GetDirectories().Length;

        }

        public List<string>? GetSubKeys(string key)
        {
            if (!Directory.Exists($"{_cachePath}"))
            {
                return null;
            }
            if (!Directory.Exists($"{_cachePath}/{key}"))
            {
                return null;
            }
            var directoryinfo = new DirectoryInfo($"{_cachePath}/{key}");
            return directoryinfo.GetDirectories().Select(x => x.Name).ToList();
        }
        public bool Remove(string key, string subKey)
        {
            if (!Directory.Exists($"{_cachePath}"))
            {
                return false;
            }
            if (!Directory.Exists($"{_cachePath}/{key}"))
            {
                return false;
            }
            if(!Directory.Exists($"{_cachePath}/{key}/{subKey}"))
            {
                return false;
            }
            if (Directory.Exists($"{_cachePath}/{key}/{subKey}"))
            {
                Directory.Delete($"{_cachePath}/{key}/{subKey}",true);
                return true;
            }
            return false;
        }

        public bool Remove(string key)
        {
            if (!Directory.Exists($"{_cachePath}"))
            {
                return false;
            }
            else
            {
                Directory.Delete($"{_cachePath}/{key}", true);
                return true;
            }
        }

        public List<Tuple<string, byte[]>>? Scan(string key)
        {

            if (!Directory.Exists($"{_cachePath}"))
            {
                return null;
            }
            if (!Directory.Exists($"{_cachePath}/{key}"))
            {
                return null;
            }
            var directoryinfo = new DirectoryInfo($"{_cachePath}/{key}");
            return directoryinfo.GetDirectories()
                    .Select(dir => new Tuple<string, byte[]>(dir.Name, 
                                                        GetCacheValues($"{_cachePath}/{key}/{dir.Name}")))
                    .ToList();
        }

        public bool Set(string key, string subKey, byte[] value, int timeToLive = 0)
        {
            if(!Directory.Exists($"{_cachePath}"))
            {
                Directory.CreateDirectory(_cachePath);
            }
            if(!Directory.Exists($"{_cachePath}/{key}"))
            {
                Directory.CreateDirectory($"{_cachePath}/{key}");
            }
            if(!Directory.Exists($"{_cachePath}/{key}/{subKey}"))
            {
                Directory.CreateDirectory($"{_cachePath}/{key}/{subKey}");
            }
            var cacheDir = $"{_cachePath}/{key}/{subKey}";
            var cacheMetadata = GetCacheMetadata(cacheDir);
            if (timeToLive > 0)
                cacheMetadata.TimeToLiveInSeconds = timeToLive;
            cacheMetadata.LastAccessed = DateTime.Now;
            cacheMetadata.Path = $"{cacheDir}/{_cacheFile}.{_cacheExtension}";
            var memoryStream = new MemoryStream(value);
            if (Trywrite(cacheMetadata.Path, 3, memoryStream))
            {
                SetCacheMetadata(cacheDir, cacheMetadata);
                return true;
            }
            else
            {
                return false;
            }

        }
        public long IncrementKey(string key, string subKey, long value = 1)
        {
            var cacheDir = $"{_cachePath}/{key}/{subKey}";
            var cacheMetadata = GetCacheMetadata(cacheDir);
            if (!Directory.Exists($"{_cachePath}"))
            {
                Directory.CreateDirectory(_cachePath);
            }
            if (!Directory.Exists($"{_cachePath}/{key}"))
            {
                Directory.CreateDirectory($"{_cachePath}/{key}");
            }
            if (!Directory.Exists($"{_cachePath}/{key}/{subKey}"))
            {
                Directory.CreateDirectory($"{_cachePath}/{key}/{subKey}");
                cacheMetadata.Path = $"{cacheDir}/{_cacheFile}.{_cacheExtension}";
            }
            
            cacheMetadata.KeyCounter += value;
            cacheMetadata.LastAccessed = DateTime.Now;
            SetCacheMetadata(cacheDir, cacheMetadata);
            return cacheMetadata.KeyCounter;


        }
        

    }
}
