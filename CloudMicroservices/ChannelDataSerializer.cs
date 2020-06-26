using BTDB.Buffer;
using BTDB.EventStore2Layer;

namespace CloudMicroServices
{
    public class ChannelDataSerializer
    {
        readonly EventSerializer _eventSerializer = new EventSerializer();
        EventDeserializer _eventDeserializer = new EventDeserializer();

        public (ByteBuffer metaData, ByteBuffer data) Serialize(object obj)
        {
            var metaData = _eventSerializer.Serialize(out var hasMetaData, obj).ToAsyncSafe();
            _eventSerializer.ProcessMetadataLog(metaData);
            return (metaData, _eventSerializer.Serialize(out hasMetaData, obj));
        }

        public object Deserialize(ByteBuffer metaData, ByteBuffer data)
        {
            _eventDeserializer.ProcessMetadataLog(metaData);
            var result = _eventDeserializer.Deserialize(out dynamic obj, data);
            return obj;
        }
    }
}