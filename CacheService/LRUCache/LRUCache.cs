using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace binary.cache.service.LRUCache;
public partial class LRUCache<TValue>
{
    private readonly long capacity;
    private long currentSize;
    private readonly Dictionary<string, LinkedListNode<CacheItem>> cache;
    private readonly LinkedList<CacheItem> lruList;
    private readonly string delimiter = "~";
    private readonly ILogger<LRUCache<TValue>> logger;
    public LRUCache(IConfiguration configuration, ILogger<LRUCache<TValue>> logger)
    {
        this.capacity = configuration.GetValue<long>("InMemoryCacheSizeInMB");
        this.cache = new Dictionary<string, LinkedListNode<CacheItem>>();
        this.lruList = new LinkedList<CacheItem>();
        this.logger = logger;

    }

    public TValue Get(string key, string subKey)
    {
        var cacheKey = $"{key}{delimiter}{subKey}";
        if (cache.TryGetValue(cacheKey, out var node))
        {
            var value = node.Value.ValueItem;
            lruList.Remove(node);
            lruList.AddLast(node);
            logger.LogDebug($"Cache hit for key: {cacheKey}");
            return value;
        }
        logger.LogDebug($"Cache miss for key: {cacheKey}");
        return default(TValue);
    }

    public void Add(string key, string subKey, TValue value, long size)
    {
        var cacheKey = $"{key}{delimiter}{subKey}";
        if (cache.ContainsKey(cacheKey))
        {
            // If the key already exists, remove it before adding the new value
            Remove(cacheKey);
            logger.LogDebug($"Cache key already exists, removing key: {cacheKey}");
        }

        while (currentSize + size > capacity)
        {
            logger.LogDebug($"Cache is full, removing least recently used item");
            // If the cache is full, remove the least recently used item
            RemoveFirst();
        }

        var cacheItem = new CacheItem { Key = key, Subkey=subKey, ValueItem = value, Size = size };
        var node = new LinkedListNode<CacheItem>(cacheItem);
        lruList.AddLast(node);
        cache.Add(cacheKey, node);
        currentSize += size;
        logger.LogDebug($"Added key: {cacheKey} to cache");
    }
    public void Update(string key, string subKey, TValue value, long size)
    {
        if (cache.TryGetValue(key, out var node))
        {
            // If the key exists, update the value and size
            currentSize -= node.Value.Size;
            node.Value.ValueItem = value;
            node.Value.Size = size;
            currentSize += size;

            // Move the updated node to the end of the list
            lruList.Remove(node);
            lruList.AddLast(node);
        }
        else
        {
            // If the key doesn't exist, add it to the cache
            Add(key, subKey, value, size);
        }
    }

    private void Remove(string key)
    {
        if (cache.TryGetValue(key, out var node))
        {
            lruList.Remove(node);
            cache.Remove(key);
            currentSize -= node.Value.Size;
        }
    }
    private void RemoveFirst()
    {
        var node = lruList.First;
        if (node != null)
        {
            var cacheKey = $"{node.Value.Key}{delimiter}{node.Value.Subkey}";
            lruList.RemoveFirst();
            cache.Remove(cacheKey);
            currentSize -= node.Value.Size;
        }
        
    }
    private class CacheItem
    {
        public string Key { get; set; }
        public string Subkey { get; set; }
        public TValue ValueItem { get; set; }
        /// <summary>
        /// size in megabytes
        /// </summary>
        public long Size { get; set; }
    }
}
