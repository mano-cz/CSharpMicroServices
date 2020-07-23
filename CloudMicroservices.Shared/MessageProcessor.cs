using System;
using BTDB.Buffer;
using BTDB.EventStore2Layer;

namespace CloudMicroservices.Shared
{
    public abstract class MessageProcessor
    {
        protected readonly EventSerializer _eventSerializer = new EventSerializer();
        protected readonly EventDeserializer _eventDeserializer = new EventDeserializer();
        protected readonly object _serializationLock = new object();

        protected (byte[] metaData, byte[] data) Serialize(object obj)
        {
            lock (_eventSerializer)
            {
                var bytes = _eventSerializer.Serialize(out var hasMetaData, obj).ToAsyncSafe();
                if (hasMetaData)
                    _eventSerializer.ProcessMetadataLog(bytes);
                else
                    return (default, bytes.ToByteArray());
                return (bytes.ToByteArray(), _eventSerializer.Serialize(out hasMetaData, obj).ToByteArray());
            }
        }

        protected object Deserialize(byte[] data)
        {
            var buffer = ByteBuffer.NewAsync(data);
            // lock (_eventDeserializer)
            // {
            var result = _eventDeserializer.Deserialize(out var obj, buffer);
            if (!result)
                throw new InvalidOperationException();
            return obj;
            // return default;
            // }
        }
    }
}