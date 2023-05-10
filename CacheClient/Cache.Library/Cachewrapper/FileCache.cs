using Microsoft.Extensions.Logging;
using static Cache.Library.DeleteCachedResponse;
using static Cache.Library.GetCachedValueResponse;

namespace Cache.Library.CacheWrapper
{
    public class FileCache : IFileCache
    {
        private readonly CacheServices.CacheServicesClient _cacheClient;
        private readonly ILogger<FileCache> _logger;
        public FileCache(CacheServices.CacheServicesClient cacheClient, ILogger<FileCache> logger)
        {
            _cacheClient = cacheClient;
            _logger = logger;
        }
        public CacheResponse Set(string key, string value, int timePeriod)
        {
            var request = new StoreCacheRequest
            {
                Key = key,
                Value = value,
                CacheDurationInMinutes = timePeriod
            };
            
            var response = _cacheClient.SetCache(request);
            return new CacheResponse
            {
                Key = response.Key,
                StatusCode = response.StatusCode,
                ErrorMessage = response.Message
            };
        }
        public CacheResponse Get(string key)
        {
            var request = new GetCachedValueRequest { Key = key };
            var response = _cacheClient.GetCache(request);
            return response.GetResultCase switch
            {
                GetResultOneofCase.CachedValue => new CacheResponse
                {
                    Key = key,
                    Value = response.CachedValue.Value
                },
                GetResultOneofCase.CacheRetrivalError => new CacheResponse
                {
                    Key = key,
                    StatusCode = response.CacheRetrivalError.ErrorCode,
                    ErrorMessage = response.CacheRetrivalError.Message
                },
                _ => new CacheResponse
                {
                    Key = key,
                },
            };
        }
        public CacheResponse Delete(string key)
        {
            var request = new DeleteCachedValueRequest { Key = key };
            var response = _cacheClient.DeleteCache(request);
            return response.DeleteResultCase switch
            {
                DeleteResultOneofCase.DeleteResponse => new CacheResponse
                {
                    Key = response.DeleteResponse.Value,
                    ErrorMessage = response.DeleteResponse.Message
                },
                DeleteResultOneofCase.CacheDeletionError => new CacheResponse
                {
                    Key = response.Key,
                    StatusCode = response.CacheDeletionError.ErrorCode,
                    ErrorMessage = response.CacheDeletionError.Message
                },
                _ => new CacheResponse
                {
                    Key = key,
                },
            };

        }
    }
}
