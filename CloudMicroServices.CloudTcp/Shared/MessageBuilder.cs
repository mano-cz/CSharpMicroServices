using System;
using System.ComponentModel.DataAnnotations;
using BTDB.Buffer;
using BTDB.StreamLayer;

namespace CloudMicroServices.CloudTcp.Shared
{
    public class MessageBuilder
    {
        ByteBuffer _messageBuffer;
        MessageType _messageType;

        const byte NewHeaderFlag = 255;

        public MessageBuilder SetMessageBuffer(ByteBuffer messaBuffer)
        {
            _messageBuffer = messaBuffer;
            return this;
        }

        public MessageBuilder SetMessageType(MessageType type)
        {
            _messageType = type;
            return this;
        }

        public byte[] Build()
        {
            Validate();
            var writer = new ByteBufferWriter();
            writer.WriteUInt8(NewHeaderFlag);
            writer.WriteUInt8((byte)_messageType);
            writer.WriteVUInt32((uint)_messageBuffer.Length); //we could allocate max 2GBs event
            writer.WriteBlock(_messageBuffer);
            return writer.Data.ToByteArray();
        }

        void Validate()
        {
            if (_messageBuffer.Length <= 0)
                throw new InvalidOperationException("Event buffer has to be set.");
        }
    }
}