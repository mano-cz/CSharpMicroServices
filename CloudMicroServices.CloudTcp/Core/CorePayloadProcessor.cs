using System;
using System.Buffers;
using BTDB.Buffer;
using CloudMicroServices.CloudTcp.Shared;

namespace CloudMicroServices.CloudTcp.Core
{
    public class CorePayloadProcessor
    {
        readonly MessageProcessor _messageProcessor;

        public CorePayloadProcessor(MessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }

        public bool ProcessPayload(ReadOnlySequence<byte> payloadSequence)
        {
            var payloadReader = new PayloadReader(payloadSequence);
            switch (payloadReader.MessageType)
            {
                case MessageType.Metadata:
                    _messageProcessor.ProcessMetadata(payloadReader.MessageBody);
                    return false;
                case MessageType.Response:
                    _messageProcessor.ProcessResponse(payloadReader.MessageBody);
                    return true;
                default:
                    throw new InvalidOperationException($"Unsupported message type {payloadReader.MessageType}.");
            }
        }

        public (ByteBuffer Meta, ByteBuffer Data) PreparePayload(object data)
        {
            return _messageProcessor.SerializeData(data);
        }
    }
}