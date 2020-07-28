using System;
using System.Buffers;
using BTDB.Buffer;
using BTDB.StreamLayer;

namespace CloudMicroServices.CloudTcp.Shared
{
    public class PayloadReader
    {
        readonly ByteBufferReader _reader;

        public MessageType MessageType { get; private set; }
        public ReadOnlyMemory<byte> MessageBody { get; private set; }

        public PayloadReader(ReadOnlySequence<byte> payloadSequence)
        {
            if (!payloadSequence.IsSingleSegment)
                throw new NotImplementedException("Only 1 payload segment supported for now.");
            var buffer = ByteBuffer.NewAsync(payloadSequence.First);
            _reader = new ByteBufferReader(buffer);
            CheckHeaderFlag();
            ReadType();
            ReadMessageBody(payloadSequence.First);
        }

        void CheckHeaderFlag()
        {
            var headerFlag = _reader.ReadUInt8();
            if (headerFlag != PayloadConstants.NewHeaderFlag) //The payload with header always have to begins with '255'
                throw new InvalidOperationException($"Payload doesn't start with NewHeaderFlag. Header flag is '{headerFlag}'");
        }

        void ReadType()
        {
            MessageType = (MessageType)_reader.ReadUInt8();
        }

        void ReadMessageBody(in ReadOnlyMemory<byte> payloadSequenceFirst)
        {
            var bodyLength = (int)_reader.ReadVUInt32();
            var bodyStart = (int)_reader.GetCurrentPosition();
            MessageBody = payloadSequenceFirst.Slice(bodyStart, bodyLength);
        }
    }
}