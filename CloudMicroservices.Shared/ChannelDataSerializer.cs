using System;
using BTDB.Buffer;
using BTDB.EventStore2Layer;

namespace CloudMicroservices.Shared
{
    public class ChannelDataSerializer
    {
        readonly EventSerializer _eventSerializer = new EventSerializer();
        readonly EventDeserializer _eventDeserializer = new EventDeserializer();

        public (ByteBuffer metaData, ByteBuffer data) Serialize(object obj)
        {
            var bytes = _eventSerializer.Serialize(out var hasMetaData, obj).ToAsyncSafe();
            if (hasMetaData)
                _eventSerializer.ProcessMetadataLog(bytes);
            else
                return (default, bytes);
            return (bytes, _eventSerializer.Serialize(out hasMetaData, obj));
        }

        public object Deserialize(ByteBuffer metaData, ByteBuffer data)
        {
            if (metaData != default)
                _eventDeserializer.ProcessMetadataLog(metaData);
            var result = _eventDeserializer.Deserialize(out var obj, data);
            if (!result)
                throw new InvalidOperationException();
            return obj;
        }
    }

    public class ChannelDataSerializer2
    {
        readonly EventSerializer _eventSerializer = new EventSerializer();
        readonly EventDeserializer _eventDeserializer = new EventDeserializer();

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

        public object Deserialize(ByteBuffer data)
        {
            lock (_eventDeserializer)
            {
                var result = _eventDeserializer.Deserialize(out var obj, data);
                if (!result)
                    throw new InvalidOperationException("Cannot deserialize object.");
                return obj;
            }
        }

        public void ProcessDeserializerMetadata(ByteBuffer meta)
        {
            lock (_eventDeserializer)
            {
                _eventDeserializer.ProcessMetadataLog(meta);
            }
        }
    }
}
