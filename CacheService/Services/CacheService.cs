using binary.cache.service.domain;
using Grpc.Core;
using System.Text;


namespace binary.cache.service.Services
{
    public class CacheService : Cache.CacheBase, IDisposable
    {
        private readonly ILogger<CacheService> _logger;
        private readonly ICacheManagement _cacheManagement;
        private readonly IConfiguration _configuration;
        private readonly IHostedService _fileCleanupService;
        private readonly CancellationToken fileCleanerToken = new CancellationToken();
        public CacheService(ILogger<CacheService> logger,
            ICacheManagement cacheManagement,
            IConfiguration configuration, IEnumerable<IHostedService> backgroundServices)
        {
            _logger = logger;
            _cacheManagement = cacheManagement;
            _configuration = configuration;
            _fileCleanupService = backgroundServices.FirstOrDefault(service => service is FileCleanupService);
            _fileCleanupService?.StartAsync(fileCleanerToken)
                               .GetAwaiter().GetResult();
        }
        public override Task<GetCachedValueResponse> GetCache(GetCachedValueRequest request, ServerCallContext context)
        {
            var response = new GetCachedValueResponse();
            try
            {
                if (string.IsNullOrEmpty(request.Key))
                {
                    throw new ArgumentNullException("Key cannot be null or empty");
                }
                if ((request.Key != string.Empty) && (request.Subkey == string.Empty))
                {
                    var scanResponse = _cacheManagement.Scan(request.Key);
                    response.SubkeyValuePairs = new SubkeyValuePairResponse();
                    if (scanResponse?.Count > 0)
                    {
                        foreach (var item in scanResponse)
                        {
                            var subkeyValuePair = new SubkeyValuePair
                            {
                                Subkey = item.Item1,
                                Value = Google.Protobuf.ByteString.CopyFrom(item.Item2)
                            };
                            response.SubkeyValuePairs.SubkeyValuePairs.Add(subkeyValuePair);
                        }
                    }

                }
                else if ((request.Key != string.Empty) && (request.Subkey != string.Empty))
                {
                    var keySubkeyResponse = _cacheManagement.Get(request.Key, request.Subkey);
                    response.CachedValue = new GetCachedValue();
                    response.CachedValue.Value = Google.Protobuf.ByteString.CopyFrom(keySubkeyResponse);
                }
                else
                {
                    response.CacheRetrivalError = new CacheError();
                    response.CacheRetrivalError.Message = "Key and Subkey pair cannot be found";
                    response.CacheRetrivalError.ErrorCode = 404;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in cache {ex.Message}\n {ex.StackTrace}");
                response.CacheRetrivalError = new CacheError();
                response.CacheRetrivalError.Message = ex.Message;
                response.CacheRetrivalError.ErrorCode = 500;
            }
            LogPodName("Get Cache");
            return Task.FromResult(response);
        }
        public override Task<StoreCacheResponse> SetCache(StoreCacheRequest request, ServerCallContext context)
        {
            var response = new StoreCacheResponse();
            try
            {
                if (string.IsNullOrEmpty(request?.CacheMessage.Key))
                {
                    throw new ArgumentNullException("Key cannot be null or empty");
                }
                if (string.IsNullOrEmpty(request?.CacheMessage.Subkey))
                {
                    throw new ArgumentNullException("Subkey cannot be null or empty");
                }
                if ((request?.CacheMessage?.Value?.IsEmpty) == true)
                {
                    throw new ArgumentNullException("Value cannot be null or empty");
                }
                var result = _cacheManagement.Set(request.CacheMessage.Key, request.CacheMessage.Subkey, request.CacheMessage.Value.ToByteArray());
                if (!result)
                {
                    response.Key = request.CacheMessage.Key;
                    response.Subkey = request.CacheMessage.Subkey;
                    response.StatusCode = 500;
                    response.Message = "Error in setting the cache";
                }
                else
                {
                    response.Key = request.CacheMessage.Key;
                    response.Subkey = request.CacheMessage.Subkey;
                    response.StatusCode = 200;
                    response.Message = "Cache set successfully";
                }
                LogPodName("Set Cache");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in cache {ex.Message}\n {ex.StackTrace}");
                response.Key = request.CacheMessage.Key;
                response.Subkey = request.CacheMessage.Subkey;
                response.StatusCode = 500;
                response.Message = ex.Message;
                LogPodName("Set Cache with Exception");
                return Task.FromResult(response);
            }

        }
        public override Task<DeleteCachedResponse> DeleteCache(DeleteCachedValueRequest request, ServerCallContext context)
        {
            var response = new DeleteCachedResponse() { Key = request.Key };
            try
            {
                if (string.IsNullOrEmpty(request.Key))
                {
                    throw new ArgumentNullException("Key cannot be null or empty");
                }
                if ((request.Key != string.Empty) && (request.Subkey == string.Empty))
                {
                    var result = _cacheManagement.Remove(request.Key);
                    if (!result)
                    {
                        response.CacheDeletionError = new CacheError();
                        response.CacheDeletionError.ErrorCode = 500;
                        response.CacheDeletionError.Message = "Error in deleting the cache";
                    }
                    else
                    {
                        response.DeleteResponse = new DeleteCachedValue();
                        response.DeleteResponse.Message = "Cache deleted successfully";
                    }
                }
                else if ((request.Key != string.Empty) && (request.Subkey != string.Empty))
                {
                    var result = _cacheManagement.Remove(request.Key, request.Subkey);
                    response.Subkey = request.Subkey;
                    if (!result)
                    {
                        response.CacheDeletionError = new CacheError();
                        response.CacheDeletionError.ErrorCode = 500;
                        response.CacheDeletionError.Message = "Error in deleting the cache";
                    }
                    else
                    {
                        response.DeleteResponse = new DeleteCachedValue();
                        response.DeleteResponse.Message = "Cache deleted successfully";
                    }
                }
                else
                {
                    response.CacheDeletionError = new CacheError();
                    response.Key = request.Key;
                    response.CacheDeletionError.ErrorCode = 404;
                    response.CacheDeletionError.Message = "Key and Subkey pair cannot be found";
                }
                LogPodName("Delete Cache");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in cache {ex.Message}\n {ex.StackTrace}");
                response.CacheDeletionError = new CacheError();
                response.CacheDeletionError.ErrorCode = 500;
                response.CacheDeletionError.Message = ex.Message;
                LogPodName("Delete Cache with Exception");
                return Task.FromResult(response);
            }
        }
        public override Task<GetAllSubkeysResponse> GetAllSubkeys(GetAllSubkeysRequest request, ServerCallContext context)
        {
            var response = new GetAllSubkeysResponse();
            try
            {
                if (string.IsNullOrEmpty(request.Key))
                {
                    throw new ArgumentNullException("Key cannot be null or empty");
                }
                var result = _cacheManagement.GetSubKeys(request.Key);
                if (result?.Count > 0)
                {
                    response.Subkeys.AddRange(result);

                }

                LogPodName("Get All Subkeys");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in cache {ex.Message}\n {ex.StackTrace}");
                LogPodName("Get All Subkeys with Exception");
                throw;
            }
        }
        public override Task<CountIncrResponse> GetSubkeyCount(GetCachedValueRequest request, ServerCallContext context)
        {
            CountIncrResponse response = new CountIncrResponse();
            try
            {
                if (string.IsNullOrEmpty(request.Key))
                {
                    throw new ArgumentNullException("Key cannot be null or empty");
                }
                var result = _cacheManagement.GetSubkeyCount(request.Key);
                response.LongValue = result;
                LogPodName("Get Subkey Count");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in cache {ex.Message}\n {ex.StackTrace}");
                LogPodName("Get Subkey Count with Exception");
                throw;
            }
        }
        public override Task<CountIncrResponse> Incr(IncrRequestMessage request, ServerCallContext context)
        {
            CountIncrResponse response = new CountIncrResponse();
            try
            {
                if (string.IsNullOrEmpty(request.Key))
                {
                    throw new ArgumentNullException("Key cannot be null or empty");
                }
                if (string.IsNullOrEmpty(request.Subkey))
                {
                    throw new ArgumentNullException("Subkey cannot be null or empty");
                }
                var result = _cacheManagement.IncrementKey(request.Key, request.Subkey, request.IncrementValue);
                response.LongValue = result;
                LogPodName("Incr");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in cache {ex.Message}\n {ex.StackTrace}");
                LogPodName("Incr with Exception");
                throw;
            }
        }
        public override Task<SetTTLResponseMessage> SetTTL(SetTTLRequestMessage request, ServerCallContext context)
        {
            var response = new SetTTLResponseMessage();
            bool result = false;
            try
            {
                if (string.IsNullOrEmpty(request.Key))
                {
                    throw new ArgumentNullException("key cannot be null or empty");
                }
                if (!string.IsNullOrWhiteSpace(request.Key) && (string.IsNullOrEmpty(request.Subkey)))
                {
                    result = _cacheManagement.SetExpiry(request.Key, request.CacheDurationInSeconds);

                }
                if (!string.IsNullOrWhiteSpace(request.Key) && (!string.IsNullOrEmpty(request.Subkey)))
                {
                    result = _cacheManagement.SetExpiry(request.Key, request.Subkey, request.CacheDurationInSeconds);
                    response.Subkey = request.Subkey;

                }

                response.Key = request.Key;


                response.StatusCode = result ? 200 : 500;
                response.Message = result ? "TTL set successfully" : "Error in setting TTL";
                LogPodName("Set TTL");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in cache {ex.Message}\n {ex.StackTrace}");
                response.Key = request.Key;
                response.Subkey = request.Subkey;
                response.StatusCode = 500;
                response.Message = ex.Message;
                LogPodName("Set TTL with Exception");
                return Task.FromResult(response);
            }
        }
        public override Task<StoreCacheResponse> SetCacheUI(SetCacheUIRequest request, ServerCallContext context)
        {
            var response = new StoreCacheResponse();
            try
            {
                if (string.IsNullOrEmpty(request?.Key))
                {
                    throw new ArgumentNullException("Key cannot be null or empty");
                }
                if (string.IsNullOrEmpty(request?.Subkey))
                {
                    throw new ArgumentNullException("Subkey cannot be null or empty");
                }
                if (string.IsNullOrEmpty(request.Value))
                {
                    throw new ArgumentNullException("Value cannot be null or empty");
                }
                byte[] contentBytes = Encoding.UTF8.GetBytes(request.Value);
                var result = _cacheManagement.Set(request.Key, request.Subkey, contentBytes);
                if (!result)
                {
                    response.Key = request.Key;
                    response.Subkey = request.Subkey;
                    response.StatusCode = 500;
                    response.Message = "Error in setting the cache";
                }
                else
                {
                    response.Key = request.Key;
                    response.Subkey = request.Subkey;
                    response.StatusCode = 200;
                    response.Message = "Cache set successfully";
                }
                LogPodName("Set Cache UI");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in cache {ex.Message}\n {ex.StackTrace}");
                response.Key = request.Key;
                response.Subkey = request.Subkey;
                response.StatusCode = 500;
                response.Message = ex.Message;
                LogPodName("Set Cache UI with Exception");
                return Task.FromResult(response);
            }

        }
        public override Task<GetValueUIResponse> GetCacheUI(GetCachedValueRequest request, ServerCallContext context)
        {
            var response = new GetValueUIResponse();
            try
            {
                if (string.IsNullOrEmpty(request.Key))
                {
                    throw new ArgumentNullException("Key cannot be null or empty");
                }
                if (string.IsNullOrWhiteSpace(request.Subkey))
                {
                    throw new ArgumentNullException("Key cannot be null or empty");
                }
                else if ((request.Key != string.Empty) && (request.Subkey != string.Empty))
                {
                    var keySubkeyResponse = _cacheManagement.Get(request.Key, request.Subkey);
                    response.CachedValue = new GetCachedValueUI();
                    response.CachedValue.CacheContent = Encoding.UTF8.GetString(keySubkeyResponse);

                }
                else
                {
                    response.CacheRetrivalError = new CacheError();
                    response.CacheRetrivalError.Message = "Key and Subkey pair cannot be found";
                    response.CacheRetrivalError.ErrorCode = 404;

                }
                LogPodName("Get Cache UI");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in cache {ex.Message}\n {ex.StackTrace}");
                response.CacheRetrivalError = new CacheError();
                response.CacheRetrivalError.Message = ex.Message;
                response.CacheRetrivalError.ErrorCode = 500;
                LogPodName("Get Cache UI with Exception");
                return Task.FromResult(response);
            }
            
        }
        private void LogPodName(string operation)
        {
            var podName = _configuration["podName"];
            _logger.LogInformation($"Operation name: {operation} Pod Name {podName}");
        }

        public void Dispose()
        {
            _fileCleanupService?.StopAsync(fileCleanerToken).Wait();
        }
    }
}
