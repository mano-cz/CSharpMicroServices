using System;
using BTDB.Buffer;
using BTDB.EventStore2Layer;

namespace CloudMicroServices
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
}