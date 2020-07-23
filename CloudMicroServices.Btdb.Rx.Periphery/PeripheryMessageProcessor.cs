using System;
using BTDB.Buffer;
using CloudMicroservices.Shared;

namespace CloudMicroServices.Btdb.Rx.Periphery
{
    public class PeripheryMessageProcessor : MessageProcessor, IMessageProcessor
    {
        IMessageProcessor _other;
        public IMessageProcessor Other
        {
            get
            {
                if (_other == null)
                    throw new InvalidOperationException("PeripheryMessageProcessor is not initialized.");
                return _other;
            }
        }

        // something like ensure initialized ... before each call maybe
        public byte[] ProcessData(byte[] data)
        {
            var nextQuery = (Query1)Deserialize(data);
            var response = new Response1 { Data = $"{nextQuery.Data}Response" };
            lock (_serializationLock)
            {
                var (meta, data2) = Serialize(response);
                if (meta != default)
                    Other.ProcessMetadata(meta);
                return data2;
            }
        }

        public void ProcessMetadata(byte[] metadata)
        {
            var buffer = ByteBuffer.NewAsync(metadata);
            lock (_eventDeserializer)
            {
                _eventDeserializer.ProcessMetadataLog(buffer);
            }
        }

        public void Initialize(IMessageProcessor other)
        {
            _other = other ?? throw new ArgumentNullException(nameof(other));
        }
    }
}