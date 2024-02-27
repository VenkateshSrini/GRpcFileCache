using ProtoBuf;

namespace binary.cache.service.domain
{
    [ProtoContract]
    public class CacheMetadata
    {
        [ProtoMember(1)]
        public string Path { get; set; }
        [ProtoMember(2)]
        public DateTime LastAccessed { get; set; }
        [ProtoMember(3)]
        public int TimeToLiveInSeconds { get; set; }
        [ProtoMember(4)]
        public long KeyCounter { get; set;}
    }
}
