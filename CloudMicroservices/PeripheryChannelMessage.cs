using BTDB.Buffer;

namespace CloudMicroServices
{
    public class PeripheryChannelMessage
    {
        public ByteBuffer Data { get; set; }
        public ByteBuffer MetaData { get; set; }
    }
}