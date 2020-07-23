using System;
using BTDB.Buffer;
using BTDB.EventStore2Layer;
using CloudMicroservices.Shared;

namespace CloudMicroServices.Btdb.Rx.Core
{
    public class CoreMessageProcessor : IMessageProcessor
    {
        readonly EventSerializer _eventSerializer = new EventSerializer();
        readonly EventDeserializer _eventDeserializer = new EventDeserializer();
        readonly object _serializationLock = new object();

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

        public void Initialize(IMessageProcessor other)
        {
            _other = other ?? throw new ArgumentNullException(nameof(other));
        }

        public Response1 ProcessQuery(Query1 query, long i)
        {
            Console.WriteLine($"{i} QUERY: {query.Data}");
            byte[] data, meta;
            lock (_serializationLock)
            {
                Console.WriteLine($"{i} Start serialization");
                (meta, data) = Serialize(query);
                if (meta != default)
                {
                    Console.WriteLine($"{i} Has new meta");
                    Other.ProcessMetadata(meta);
                }
                Console.WriteLine($"{i} End serialization");
            }
            Console.WriteLine($"{i} Deserialization START");
            var responseData = Other.ProcessData(data);
            var response = (Response1)Deserialize(responseData);
            Console.WriteLine($"{i} Deserialization END");
            return response;
        }

        public byte[] ProcessData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void ProcessMetadata(byte[] metadata)
        {
            var buffer = ByteBuffer.NewAsync(metadata);
            lock (_eventDeserializer)
            {
                _eventDeserializer.ProcessMetadataLog(buffer);
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

        public (byte[] metaData, byte[] data) Serialize(object obj)
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
    }
}