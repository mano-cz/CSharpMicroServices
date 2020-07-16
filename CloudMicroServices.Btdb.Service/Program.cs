using System;
using System.Threading;
using System.Threading.Tasks;
using BTDB.Buffer;
using BTDB.Service;
using CloudMicroservices.Shared;

namespace CloudMicroServices.Btdb.Rx.Core
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var pipedTwoChannels = new PipedTwoChannels();
            var coreSerializer = new ChannelDataSerializer();
            var coreService = new Service(pipedTwoChannels.First);
            var periphery = new Periphery.Periphery(pipedTwoChannels.Second);
            var random = new Random();
            var serializerLock = new object();
            var forResult = Parallel.For(0, int.MaxValue, (i, state) =>
            {
                var query = new Query1(new string('a', random.Next(1, 5)));
                var peripheryMessageProcessor = coreService.QueryRemoteService<Func<PeripheryChannelMessage, PeripheryChannelMessage>>();
                ByteBuffer data, meta;
                PeripheryChannelMessage result;
                lock (serializerLock)
                {
                    (meta, data) = coreSerializer.Serialize(query);
                    result = peripheryMessageProcessor(new PeripheryChannelMessage
                    {
                        Data = data,
                        MetaData = meta
                    });
                }
                var response = (Response1)coreSerializer.Deserialize(result.MetaData, result.Data);
                Console.WriteLine($"Received response to query {query.Data} with data {response.Data}");
            });
            await Task.Delay(Timeout.Infinite);
        }
    }
}
