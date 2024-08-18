using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using binary.cache.service;
using Microsoft.Extensions.Logging;
namespace cache.library.CacheFacade
{
    public class CacheProvider : ICacheProvider
    {
        protected readonly Cache.CacheClient _client;
        private readonly ILogger<CacheProvider> _logger;
        public CacheProvider(Cache.CacheClient client, ILogger<CacheProvider> logger)
        {
            _client = client;
            _logger = logger;
        }
        public CacheResponse<byte[]> Get(string key, string subKey)
        {
            GetCachedValueRequest request = new GetCachedValueRequest
            {
                Key = key,
                Subkey = subKey
            };
            try
            {
                var response = _client.GetCache(request);
                return response.GetResultCase switch
                {
                    GetCachedValueResponse.GetResultOneofCase.CachedValue => new CacheResponse<byte[]>
                    {
                        Key = response.Key,
                        SubKey = response.Subkey,
                        Value = response.CachedValue.Value.ToArray(),
                        StatusCode = 200
                    },
                    GetCachedValueResponse.GetResultOneofCase.CacheRetrivalError => new CacheResponse<byte[]>
                    {
                        Key = response.Key,
                        SubKey = response.Subkey,
                        StatusCode = 500,
                        ErrorMessage = response.CacheRetrivalError.Message
                    },
                    _ => new CacheResponse<byte[]>
                    {
                        Key = key,
                        SubKey = subKey,
                        StatusCode = 500,
                        ErrorMessage = "Unknown error"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<byte[]>
                {
                    Key = key,
                    SubKey = subKey,
                    StatusCode = 500,
                    ErrorMessage = ex.Message
                };
            }
        }
        public CacheResponse<List<(string subKey, byte[] cacheValue)>> Scan(string key)
        {
            GetCachedValueRequest request = new GetCachedValueRequest
            {
                Key = key
            };
            try
            {
                var response = _client.GetCache(request);
                return response.GetResultCase switch
                {
                    GetCachedValueResponse.GetResultOneofCase.SubkeyValuePairs => new CacheResponse<List<(string subKey, byte[] cacheValue)>>
                    {

                        Key = response.Key,
                        Value = response.SubkeyValuePairs.SubkeyValuePairs.Select(x => (x.Subkey, x.Value.ToArray())).ToList(),
                        StatusCode = 200
                    },
                    GetCachedValueResponse.GetResultOneofCase.CacheRetrivalError => new CacheResponse<List<(string subKey, byte[] cacheValue)>>
                    {
                        Key = response.Key,
                        StatusCode = 500,
                        ErrorMessage = response.CacheRetrivalError.Message
                    },
                    _ => new CacheResponse<List<(string subKey, byte[] cacheValue)>>
                    {
                        Key = key,
                        StatusCode = 500,
                        ErrorMessage = "Unknown error"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<List<(string subKey, byte[] cacheValue)>>
                {
                    Key = key,
                    StatusCode = 500,
                    ErrorMessage = ex.Message
                };
            }
        }
        public CacheResponse<long> GetSubkeyCount(string key)
        {
            GetCachedValueRequest request = new GetCachedValueRequest
            {
                Key = key
            };
            try
            {
                var response = _client.GetSubkeyCount(request);
                return new CacheResponse<long>
                {
                    Key = key,
                    Value = response.LongValue,
                    StatusCode = 200
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<long>
                {
                    Key = key,
                    ErrorMessage = ex.Message,
                    StatusCode = 500
                }; ;
            }
        }

        public CacheResponse<List<string>>? GetSubKeys(string key)
        {
            var request = new GetAllSubkeysRequest
            {
                Key = key
            };

            try
            {
                var response = _client.GetAllSubkeys(request);
                return new CacheResponse<List<string>>
                {
                    Key = key,
                    Value = response.Subkeys.ToList(),
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<List<string>>
                {
                    Key = key,
                    ErrorMessage = ex.Message,
                    StatusCode = 500
                }; ;
            }
        }

        public CacheResponse<long> IncrementKey(string key, string subKey, long value = 1)
        {
            var request = new IncrRequestMessage
            {
                Payload = new IncrementPayload
                {
                    Key = key,
                    Subkey = subKey,
                    IncrementValue = value
                }
            };
            try
            {
                var response = _client.Incr(request);
                return new CacheResponse<long>
                {
                    Key = key,
                    SubKey = subKey,
                    Value = response.LongValue,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<long>
                {
                    Key = key,
                    SubKey = subKey,
                    ErrorMessage = ex.Message,
                    StatusCode = 500
                }; ;
            }
        }

        public CacheResponse<string> Remove(string key, string subKey)
        {
            var request = new DeleteCachedValueRequest { Key = key, Subkey = subKey };
            try
            {
                var response = _client.DeleteCache(request);
                return response.DeleteResultCase switch
                {
                    DeleteCachedResponse.DeleteResultOneofCase.DeleteResponse => new CacheResponse<string>
                    {
                        Key = key,
                        SubKey = subKey,
                        Value = response.DeleteResponse.Message,
                        StatusCode = 200
                    },
                    DeleteCachedResponse.DeleteResultOneofCase.CacheDeletionError => new CacheResponse<string>
                    {
                        Key = key,
                        SubKey = subKey,
                        StatusCode = 500,
                        ErrorMessage = response.CacheDeletionError.Message
                    },
                    _ => new CacheResponse<string>
                    {
                        Key = key,
                        SubKey = subKey,
                        StatusCode = 500,
                        ErrorMessage = "Unknown error"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<string>
                {
                    Key = key,
                    SubKey = subKey,
                    ErrorMessage = ex.Message,
                    StatusCode = 500
                }; ;
            }
        }

        public CacheResponse<string> Remove(string key)
        {
            return Remove(key, "");
        }
        public CacheResponse<bool> Set(string key, string subKey, byte[] value, int timeToLive = 0)
        {
            var request = new StoreCacheRequest
            {
                CacheMessage = new CacheMessage
                {
                    Key = key,
                    Subkey = subKey,
                    Value = Google.Protobuf.ByteString.CopyFrom(value),
                    CacheDurationInSeconds = timeToLive
                }
            };
            try
            {
                var response = _client.SetCache(request);
                return new CacheResponse<bool>
                {
                    Key = key,
                    SubKey = subKey,
                    StatusCode = 200,
                    Value = (response.StatusCode == 200) ? true : false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<bool>
                {
                    Key = key,
                    SubKey = subKey,
                    StatusCode = 500,
                    ErrorMessage = ex.Message,
                    Value = false
                };
            }

        }

        public CacheResponse<bool> SetExpiry(string key, string subKey, int timeToLiveInSeconds)
        {
            var request = new SetTTLRequestMessage
            {
                Payload = new TTLPayLoad
                {
                    Key = key,
                    Subkey = subKey,
                    CacheDurationInSeconds = timeToLiveInSeconds
                }
            };
            try
            {
                var response = _client.SetTTL(request);
                return new CacheResponse<bool>
                {
                    Key = key,
                    SubKey = subKey,
                    StatusCode = 200,
                    Value = (response.StatusCode == 200) ? true : false
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<bool>
                {
                    Key = key,
                    SubKey = subKey,
                    StatusCode = 500,
                    ErrorMessage = ex.Message,
                    Value = false
                };
            }
        }

        public CacheResponse<bool> SetExpiry(string key, int timeToLiveInSeconds)
        {
            return SetExpiry(key, "", timeToLiveInSeconds);
        }
        public CacheResponse<bool> StoreCacheString(string key, string subKey, string value, int timeToLive = 0)
        {
            var request = new SetCacheStringRequest
            {
                Payload = new StrigCacheMessagePayload
                {
                    Key = key,
                    Subkey = subKey,
                    Value = value,
                    CacheDurationInSeconds = timeToLive
                }
            };
            try
            {
                var response = _client.SetCacheString(request);
                return new CacheResponse<bool>
                {
                    Key = key,
                    SubKey = subKey,
                    StatusCode = 200,
                    Value = (response.StatusCode == 200) ? true : false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<bool>
                {
                    Key = key,
                    SubKey = subKey,
                    StatusCode = 500,
                    ErrorMessage = ex.Message,
                    Value = false
                };
            }
        }
        public CacheResponse<string> GetCacheString(string key, string subKey)
        {
            var request = new GetCachedValueRequest
            {
                Key = key,
                Subkey = subKey
            };
            try
            {
                var response = _client.GetCacheString(request);
                return response.GetResultCase switch
                {
                    GetValueStringResponse.GetResultOneofCase.CachedValue => new CacheResponse<string>
                    {
                        Key = response.Key,
                        SubKey = response.Subkey,
                        Value = response.CachedValue.CacheContent,
                        StatusCode = 200
                    },
                    GetValueStringResponse.GetResultOneofCase.CacheRetrivalError => new CacheResponse<string>
                    {
                        Key = response.Key,
                        SubKey = response.Subkey,
                        StatusCode = 500,
                        ErrorMessage = response.CacheRetrivalError.Message
                    },
                    _ => new CacheResponse<string>
                    {
                        Key = key,
                        SubKey = subKey,
                        StatusCode = 500,
                        ErrorMessage = "Unknown error"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting the value from cache");
                return new CacheResponse<string>
                {
                    Key = key,
                    SubKey = subKey,
                    StatusCode = 500,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
