﻿using System;
using BTDB.Buffer;
using BTDB.EventStore2Layer;
using CloudMicroservices.Shared;

namespace CloudMicroServices.Btdb.Rx.Periphery
{
    public class PeripheryMessageProcessor : IMessageProcessor
    {
        // readonly Func<IMessageProcessor> _coreMessageProcessorFactory;
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

        // public PeripheryMessageProcessor(Func<IMessageProcessor> coreMessageProcessorFactory)
        // {
        // _coreMessageProcessorFactory = coreMessageProcessorFactory;
        // }

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

        public object Deserialize(byte[] data)
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

        public void Initialize(IMessageProcessor other)
        {
            _other = other ?? throw new ArgumentNullException(nameof(other));
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