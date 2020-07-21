using BTDB.Buffer;

namespace CloudMicroservices.Shared
{
    public class ChannelMessage
    {
        public ByteBuffer Payload { get; set; }
        public ChannelMessagePayloadType Type { get; set; }
    }

    public enum ChannelMessagePayloadType
    {
        Data,
        Metadata,
        MetadataProcessed
    }
}