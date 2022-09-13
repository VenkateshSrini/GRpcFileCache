namespace Cache.Library.CacheWrapper
{
    public class CacheResponse
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
