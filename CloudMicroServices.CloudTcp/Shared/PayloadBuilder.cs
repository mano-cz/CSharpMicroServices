using System;
using BTDB.Buffer;
using BTDB.StreamLayer;

namespace CloudMicroServices.CloudTcp.Shared
{
    public class PayloadBuilder
    {
        ByteBuffer _messageBuffer;
        MessageType _messageType;

        public PayloadBuilder SetMessageBuffer(ByteBuffer messageBuffer)
        {
            _messageBuffer = messageBuffer;
            return this;
        }

        public PayloadBuilder SetMessageType(MessageType messageType)
        {
            _messageType = messageType;
            return this;
        }

        public byte[] Build()
        {
            Validate();
            var writer = new ByteBufferWriter();
            writer.WriteUInt8(PayloadConstants.NewHeaderFlag);
            writer.WriteUInt8((byte)_messageType);
            writer.WriteVUInt32((uint)_messageBuffer.Length); //we could allocate max 2GBs event
            writer.WriteBlock(_messageBuffer);
            return writer.Data.ToByteArray();
        }

        void Validate()
        {
            if (_messageBuffer.Length <= 0)
                throw new InvalidOperationException("Payload buffer has to be set.");
        }
    }
}