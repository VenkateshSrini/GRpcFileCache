namespace binary.cache.service.domain
{
    public interface ICacheManagement
    {
        /// <summary>
        /// Gets the byte Array pertaining to the key and subkey
        /// </summary>
        /// <param name="key">Main key</param>
        /// <param name="subKey">Sub key</param>
        /// <returns>byte array</returns>
        byte[] Get(string key, string subKey);
        /// <summary>
        /// Sets the byte array to the key and subkey
        /// </summary>
        /// <param name="key">Main Key</param>
        /// <param name="subKey">Sub key</param>
        /// <param name="value">Cache value</param>
        /// <returns></returns>
        bool Set(string key, string subKey, byte[] value, int timeToLive=0);
        /// <summary>
        /// Remove the subkey and value of the main key
        /// </summary>
        /// <param name="key">main key</param>
        /// <param name="subKey">sub key</param>
        /// <returns> Value indicating success or failure</returns>
        bool Remove(string key, string subKey);
        /// <summary>
        /// Remove the main key and all its subkeys
        /// </summary>
        /// <param name="key">Main key</param>
        /// <returns>Value indicating success or failure</returns>
        bool Remove(string key);
        /// <summary>
        /// Gets the list of subkeys for the main key
        /// </summary>
        /// <param name="key"> Main Key</param>
        /// <returns></returns>
        List<string>? GetSubKeys(string key);
        /// <summary>
        /// Gets the list of subkeys keys and their values
        /// </summary>
        /// <param name="key">Main Key</param>
        /// <returns>Returns the subkey and values</returns>
        List<Tuple<string, byte[]>>? Scan(string key);
        /// <summary>
        /// Gets the count of subkeys for the main key
        /// </summary>
        /// <param name="key">Main key</param>
        /// <returns>Count of subkey</returns>
        long GetSubkeyCount(string key);
        /// <summary>
        /// Increments the key by the value
        /// </summary>
        /// <param name="key">Main key</param>
        /// <param name="subKey">Sub key</param>
        /// <param name="value">To increment by given value. If the value is not given
        /// the defaut value is 1</param>
        /// <returns>retruns the </returns>
        long IncrementKey(string key, string subKey, long value=1);
        /// <summary>
        /// Set the expiry for the key and subkey
        /// </summary>
        /// <param name="key">Main Key</param>
        /// <param name="subKey">Sub key</param>
        /// <param name="timeToLiveInSeconds"> time value in Seconds</param>
        /// <returns></returns>
        bool SetExpiry(string key, string subKey, int timeToLiveInSeconds);
        /// <summary>
        /// Set the expiry for the key
        /// </summary>
        /// <param name="key">Main Key</param>
        /// <param name="timeToLiveInSeconds">time value in Second</param>
        /// <returns></returns>
        bool SetExpiry(string key, int timeToLiveInSeconds);
    }
}
