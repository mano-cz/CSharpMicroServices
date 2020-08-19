using System;
using BTDB.Buffer;
using CloudMicroServices.CloudTcp.Periphery;

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

        public (ByteBuffer Meta, ByteBuffer Data) ProcessQuery(ReadOnlyMemory<byte> queryBytes)
        {
            var query = (Query1)_messageSerializer.Deserialize(queryBytes);
            var response = new Response1 { Data = $"{query.Data}Response" };
            return SerializeData(response);
        }

        public void ProcessResponse(ReadOnlyMemory<byte> responseBytes)
        {
            var response = (Response1)_messageSerializer.Deserialize(responseBytes);
            Console.WriteLine($"Response processed `{response.Data}`.");
        }

        public (ByteBuffer Meta, ByteBuffer Data) SerializeData(object data)
        {
            return _messageSerializer.Serialize(data);
        }
    }
}