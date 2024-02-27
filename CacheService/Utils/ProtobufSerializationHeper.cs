using ProtoBuf;

namespace binary.cache.service.Utils
{
    public static class ProtobufSerializationHeper
    {
        private static SemaphoreSlim semaphoreSlimforSerialization = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim semaphoreSlimforDeserialization = new SemaphoreSlim(1, 1);
        public static  MemoryStream Serialize<T>(T obj)
        {

            semaphoreSlimforSerialization.Wait();
            var memoryStream = new MemoryStream();
            try
            {
                Serializer.Serialize(memoryStream, obj);
                memoryStream.Position = 0;
                return memoryStream;
            }
            finally
            {
                semaphoreSlimforSerialization.Release();
            }
        }
        public static  T Deserialize<T>(Stream stream)
        {
             semaphoreSlimforDeserialization.Wait();
            try
            {
                return Serializer.Deserialize<T>(stream);
            }
            finally
            {
                semaphoreSlimforDeserialization.Release();
            }
        }
    }
}
