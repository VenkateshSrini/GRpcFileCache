using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cache.library.CacheFacade
{
    public interface ICacheProvider
    {
        /// <summary>
        /// Gets the byte Array pertaining to the key and subkey
        /// </summary>
        /// <param name="key">Main key</param>
        /// <param name="subKey">Sub key</param>
        /// <returns>byte array</returns>
        CacheResponse<byte[]> Get(string key, string subKey);
        /// <summary>
        /// Gets the byte Array pertaining to the key and subkey
        /// </summary>
        /// <param name="key">Main key</param>
        /// <param name="subKey">Sub key</param>
        /// <returns>byte array</returns>
        Task<CacheResponse<byte[]>> GetAsync(string key, string subKey);
        /// <summary>
        /// Gets the list of subkeys and their values for the main key
        /// </summary>
        /// <param name="key"> Key whose Value needs to be retrieved</param>
        /// <returns>Cache response containing the List of Tuples of subkey and byte array</returns>
        CacheResponse<List<(string subKey, byte[]cacheValue)>>Scan(string key);
        /// <summary>
        /// Gets the list of subkeys and their values for the main key
        /// </summary>
        /// <param name="key"> Key whose Value needs to be retrieved</param>
        /// <returns>Cache response containing the List of Tuples of subkey and byte array</returns>
        Task<CacheResponse<List<(string subKey, byte[] cacheValue)>>> ScanAsync(string key);
        /// <summary>
        /// Sets the byte array to the key and subkey
        /// </summary>
        /// <param name="key">Main Key</param>
        /// <param name="subKey">Sub key</param>
        /// <param name="value">Cache value</param>
        /// <returns></returns>
        CacheResponse<bool> Set(string key, string subKey, byte[] value, int timeToLive = 0);
        /// <summary>
        /// Sets the byte array to the key and subkey
        /// </summary>
        /// <param name="key">Main Key</param>
        /// <param name="subKey">Sub key</param>
        /// <param name="value">Cache value</param>
        /// <returns></returns>
        Task<CacheResponse<bool>> SetAsync(string key, string subKey, byte[] value, int timeToLive = 0);
        /// <summary>
        /// Remove the subkey and value of the main key
        /// </summary>
        /// <param name="key">main key</param>
        /// <param name="subKey">sub key</param>
        /// <returns> Value indicating success or failure</returns>
        CacheResponse<string> Remove(string key, string subKey);
        /// <summary>
        /// Remove the subkey and value of the main key
        /// </summary>
        /// <param name="key">main key</param>
        /// <param name="subKey">sub key</param>
        /// <returns> Value indicating success or failure</returns>
        Task<CacheResponse<string>> RemoveAsync(string key, string subKey);
        /// <summary>
        /// Remove the main key and all its subkeys
        /// </summary>
        /// <param name="key">Main key</param>
        /// <returns>Value indicating success or failure</returns>
        CacheResponse<string> Remove(string key);
        /// <summary>
        /// Remove the main key and all its subkeys
        /// </summary>
        /// <param name="key">Main key</param>
        /// <returns>Value indicating success or failure</returns>
        Task<CacheResponse<string>> RemoveAsync(string key);
        /// <summary>
        /// Gets the list of subkeys for the main key
        /// </summary>
        /// <param name="key"> Main Key</param>
        /// <returns></returns>
        CacheResponse<List<string>>? GetSubKeys(string key);
        /// <summary>
        /// Gets the list of subkeys for the main key
        /// </summary>
        /// <param name="key"> Main Key</param>
        /// <returns></returns>
        Task<CacheResponse<List<string>>>? GetSubKeysAsync(string key);

        /// <summary>
        /// Gets the count of subkeys for the main key
        /// </summary>
        /// <param name="key">Main key</param>
        /// <returns>Count of subkey</returns>
        CacheResponse<long> GetSubkeyCount(string key);
        /// <summary>
        /// Increments the key by the value
        /// </summary>
        /// <param name="key">Main key</param>
        /// <param name="subKey">Sub key</param>
        /// <param name="value">To increment by given value. If the value is not given
        /// the defaut value is 1</param>
        /// <returns>retruns the </returns>
        CacheResponse<long> IncrementKey(string key, string subKey, long value = 1);
        /// <summary>
        /// Set the expiry for the key and subkey
        /// </summary>
        /// <param name="key">Main Key</param>
        /// <param name="subKey">Sub key</param>
        /// <param name="timeToLiveInSeconds"> time value in Seconds</param>
        /// <returns></returns>
        CacheResponse<bool> SetExpiry(string key, string subKey, int timeToLiveInSeconds);
        /// <summary>
        /// Set the expiry for the key
        /// </summary>
        /// <param name="key">Main Key</param>
        /// <param name="timeToLiveInSeconds">time value in Second</param>
        /// <returns></returns>
        CacheResponse<bool> SetExpiry(string key, int timeToLiveInSeconds);
        CacheResponse<bool> StoreCacheString(string key, string subKey, string value, int timeToLive = 0);
        CacheResponse<string> GetCacheString(string key, string subKey );
    }
}
