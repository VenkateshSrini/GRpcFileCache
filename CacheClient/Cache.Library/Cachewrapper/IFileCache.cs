namespace Cache.Library.CacheWrapper
{
    public interface IFileCache
    {
        CacheResponse Delete(string key);
        CacheResponse Get(string key);
        CacheResponse Set(string key, string value, int timePeriod);
    }
}