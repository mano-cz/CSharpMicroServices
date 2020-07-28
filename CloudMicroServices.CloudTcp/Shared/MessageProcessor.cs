using System;

namespace CloudMicroServices.CloudTcp.Shared
{
    public class MessageProcessor
    {
        readonly MessageSerializer _messageSerializer;

        public MessageProcessor(MessageSerializer messageSerializer)
        {
            _messageSerializer = messageSerializer;
        }

        public void ProcessMetadata(ReadOnlyMemory<byte> metadata)
        {
            _messageSerializer.ApplyMetadataToDeserializer(metadata);
        }
    }
}