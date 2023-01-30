using CacheService.Caching;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using Net.DistributedFileStoreCache;

namespace CacheService.Services
{
    public class CacheService : CacheServices.CacheServicesBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CacheService> _logger;
        private readonly IDistributedFileStoreCacheString _cache;
        public CacheService(IConfiguration configuration,
            ILogger<CacheService> logger, IDistributedFileStoreCacheString cache)
        {
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
        }
        public override Task<StoreCacheResponse> SetCache(StoreCacheRequest request, ServerCallContext context)
        {
            var timePeriod = _configuration["TimePeriodinMinutes"];

            StoreCacheResponse response = new();
            if (string.IsNullOrWhiteSpace(request.CacheMessage.Key))
            {
                response.StatusCode = 400;
                response.Message = "Key is empty";
            }
            else if (string.IsNullOrWhiteSpace(request.CacheMessage.Value))
            {
                response.StatusCode = 400;
                response.Message = "Value is empty";
            }
            else
            {
                TimeSpan expiration;
                if (request.CacheMessage.CacheDurationInMinutes == 0)
                    expiration = new TimeSpan(0,
                        int.Parse(timePeriod), 0);
                else
                    expiration = new TimeSpan(0,
                        request.CacheMessage.CacheDurationInMinutes, 0); ;

                _cache.Set(request?.CacheMessage.Key, request?.CacheMessage.Value,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration
                    });
                response.Key = request.CacheMessage.Key;
                response.StatusCode = 200;
                response.Message = "success";
            }
            LogPodName("Set Cache");
            return Task.FromResult(response);
        }
        public override Task<GetCachedValueResponse> GetCache(GetCachedValueRequest request, ServerCallContext context)
        {
            GetCachedValueResponse response = new();
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                response.CacheRetrivalError = new CacheError
                {
                    ErrorCode = 400,
                    Message = "key not given"
                };

            }
            else
            {
                var cacheResponse = _cache.Get(request.Key);
                response.CachedValue = new GetCachedValue
                {
                    Value = cacheResponse,

                };
            }
            LogPodName("Get Cache");
            return Task.FromResult(response);
        }
        public override Task<DeleteCachedResponse> DeleteCache(DeleteCachedValueRequest request, ServerCallContext context)
        {
            DeleteCachedResponse response = new();
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                response.CacheDeletionError = new CacheError
                {
                    ErrorCode = 400,
                    Message = "key not given"
                };
            }
            else
            {
                var cacheResponse = _cache.Get(request.Key);
                _cache.Remove(request.Key);
                response.DeleteResponse = new DeleteCachedValue
                {
                    Message = "Item removed successful",
                    Value = cacheResponse
                };
            }
            LogPodName("Delete Cache");
            return Task.FromResult(response);
        }
        private void LogPodName(string opration) {
            var podName = _configuration["podName"];
            _logger.LogInformation($"Operation name: {opration} Pod Name {podName}");
        }
    }
}
