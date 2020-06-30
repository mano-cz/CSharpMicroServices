using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BTDB.EventStore2Layer;
using BTDB.ODBLayer;

namespace CloudMicroServices
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var random = new Random();
            var peripheryChannels = new PeripheryChannels();
            var periphery = new Periphery(peripheryChannels);
            var peripheryThread = new Thread(periphery.Start)
            {
                IsBackground = true,
                Name = nameof(periphery)
            };
            peripheryThread.Start();

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                var query = new Query1(new string('a', random.Next(1, 5)));


                // test point 2 point without metadata send - endpoint can load its own metadata on start
                // EventSerializer _eventSerializer = new EventSerializer();
                // EventDeserializer _eventDeserializer = new EventDeserializer();
                // var metaData = _eventSerializer.Serialize(out var hasMetaData, query).ToAsyncSafe();
                // _eventSerializer.ProcessMetadataLog(metaData);
                // var result1 = (metaData, _eventSerializer.Serialize(out hasMetaData, query));
                // // _eventDeserializer.ProcessMetadataLog(metaData);
                // var result2 = _eventDeserializer.Deserialize(out var obj, result1.Item2);
                // if (!result2)
                // {
                //     result2 = _eventDeserializer.Deserialize(out obj, result1.Item2);
                // }
                // var dQuery = (Query1)obj;

                // var serializer2 = new ChannelDataSerializer();
                // var (meta2, data2) = serializer2.Serialize(query);
                // var querySecond = (Query1)serializer2.Deserialize(meta2, data2);

                var correlationId = (ulong)random.Next();
                var outputChannel = peripheryChannels.Output.AddOrUpdate(
                    correlationId,
                    key => Channel.CreateBounded<PeripheryChannelMessage>(1),
                    (key, channel) => Channel.CreateBounded<PeripheryChannelMessage>(1));
                var (meta, data) = peripheryChannels.Serializer.Serialize(query);
                await peripheryChannels.Input.Writer.WriteAsync(new PeripheryInputChannelMessage
                {
                    CorrelationId = correlationId,
                    Data = data,
                    MetaData = meta
                });
                var responseMessage = await outputChannel.Reader.ReadAsync();
                var response = (Response1)peripheryChannels.Serializer.Deserialize(responseMessage.MetaData, responseMessage.Data);
                peripheryChannels.Output.Remove(correlationId, out outputChannel);
                // outputChannel.Writer.Complete();
                Console.WriteLine($"Received response to query {query.Data} with data {response.Data}");
            }
        }
    }
}
