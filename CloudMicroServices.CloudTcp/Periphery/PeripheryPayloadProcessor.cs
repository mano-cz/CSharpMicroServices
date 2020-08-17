﻿using System;
using System.Buffers;
using BTDB.Buffer;
using CloudMicroServices.CloudTcp.Shared;

namespace CloudMicroServices.CloudTcp.Periphery
{
    public class PeripheryPayloadProcessor
    {
        readonly MessageProcessor _messageProcessor;

        public PeripheryPayloadProcessor(MessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
        }

        public (ByteBuffer Meta, ByteBuffer Data) ProcessPayload(ReadOnlySequence<byte> payloadSequence)
        {
            var payloadReader = new PayloadReader(payloadSequence);
            switch (payloadReader.MessageType)
            {
                case MessageType.Metadata:
                    _messageProcessor.ProcessMetadata(payloadReader.MessageBody);
                    break;
                case MessageType.Query:
                    return _messageProcessor.ProcessQuery(payloadReader.MessageBody);
                default:
                    throw new InvalidOperationException($"Unsupported message type {payloadReader.MessageType}.");
            }
            return default;
        }
    }
}