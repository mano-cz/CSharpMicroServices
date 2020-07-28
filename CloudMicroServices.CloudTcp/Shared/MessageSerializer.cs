using System;
using BTDB.Buffer;
using BTDB.EventStore2Layer;

namespace CloudMicroServices.CloudTcp.Shared
{
    public class MessageSerializer
    {
        readonly EventSerializer _eventSerializer = new EventSerializer();
        readonly EventDeserializer _eventDeserializer = new EventDeserializer();
        // protected readonly object _serializationLock = new object();

        public (ByteBuffer metaData, ByteBuffer data) Serialize(object obj)
        {
            lock (_eventSerializer)
            {
                var bytes = _eventSerializer.Serialize(out var hasMetaData, obj).ToAsyncSafe();
                if (hasMetaData)
                    _eventSerializer.ProcessMetadataLog(bytes);
                else
                    return (default, bytes);
                return (bytes, _eventSerializer.Serialize(out hasMetaData, obj));
            }
        }

        public object Deserialize(byte[] data)
        {
            var buffer = ByteBuffer.NewAsync(data);
            // lock (_eventDeserializer)
            // {
            var result = _eventDeserializer.Deserialize(out var obj, buffer);
            if (!result)
                throw new InvalidOperationException();
            return obj;
            // }
        }

        public void ApplyMetadataToDeserializer(ReadOnlyMemory<byte> metadata)
        {
            var buffer = ByteBuffer.NewAsync(metadata);
            lock (_eventDeserializer) // maybe lock not needed
            {
                _eventDeserializer.ProcessMetadataLog(buffer);
            }
        }
    }
}