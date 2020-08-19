using System;
using System.Buffers;
using BTDB.Buffer;
using CloudMicroServices.CloudTcp.Shared;

namespace CloudMicroServices.CloudTcp.Periphery
{
    public class PeripheryPayloadProcessor
    {
        readonly MessageSerializer _messageSerializer;

        public PeripheryPayloadProcessor(MessageSerializer messageSerializer)
        {
            _messageSerializer = messageSerializer;
        }

        public (ByteBuffer Meta, ByteBuffer Data) ProcessPayload(ReadOnlySequence<byte> payloadSequence)
        {
            var payloadReader = new PayloadReader(payloadSequence);
            switch (payloadReader.MessageType)
            {
                case MessageType.Metadata:
                    _messageSerializer.ApplyMetadataToDeserializer(payloadReader.MessageBody);
                    break;
                case MessageType.Query:
                    var query = (Query1)_messageSerializer.Deserialize(payloadReader.MessageBody);
                    var response = new Response1 { Data = $"{query.Data}Response" };
                    return _messageSerializer.Serialize(response);
                default:
                    throw new InvalidOperationException($"Unsupported message type {payloadReader.MessageType}.");
            }
            return default;
        }
    }
}