using BTDB.Buffer;

namespace CloudMicroservices.Shared
{
    public class PeripheryChannelMessage
    {
        public ByteBuffer Data { get; set; }
        public ByteBuffer MetaData { get; set; }
    }
}