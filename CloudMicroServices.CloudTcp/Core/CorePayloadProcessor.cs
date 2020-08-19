using System;
using System.Buffers;
using BTDB.Buffer;
using CloudMicroServices.CloudTcp.Shared;

namespace CloudMicroServices.CloudTcp.Core
{
    public class CorePayloadProcessor
    {
        readonly MessageSerializer _messageSerializer;

        public CorePayloadProcessor(MessageSerializer messageSerializer)
        {
            _messageSerializer = messageSerializer;
        }

        public object ProcessPayload(ReadOnlySequence<byte> payloadSequence)
        {
            var payloadReader = new PayloadReader(payloadSequence);
            switch (payloadReader.MessageType)
            {
                case MessageType.Metadata:
                    _messageSerializer.ApplyMetadataToDeserializer(payloadReader.MessageBody);
                    return default;
                case MessageType.Response:
                    return _messageSerializer.Deserialize(payloadReader.MessageBody);
                default:
                    throw new InvalidOperationException($"Unsupported message type {payloadReader.MessageType}.");
            }
        }

        public (ByteBuffer Meta, ByteBuffer Data) PreparePayload(object data)
        {
            return _messageSerializer.Serialize(data);
        }
    }
}