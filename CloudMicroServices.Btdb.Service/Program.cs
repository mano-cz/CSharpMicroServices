using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Running;
using BTDB.Buffer;
using BTDB.Service;
using CloudMicroservices.Shared;

namespace CloudMicroServices.Btdb.Rx.Core
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SeriesVsParallelCore>();
        }

        [MemoryDiagnoser]
        [RankColumn(NumeralSystem.Stars)]
        [JsonExporterAttribute.Full]
        public class SeriesVsParallelCore
        {
            readonly int _testQueryCount = 10;

            [Benchmark]
            public void SeriesTest()
            {
                // var pipedTwoChannels = new PipedTwoChannels();
                // var coreSerializer = new ChannelDataSerializer();
                // var coreService = new Service(pipedTwoChannels.First);
                // var periphery = new Periphery.Periphery(pipedTwoChannels.Second);
                // var random = new Random();
                // var peripheryMessageProcessor = coreService.QueryRemoteService<Func<ChannelMessage, ChannelMessage>>();
                // for (int i = 0; i < _testQueryCount; i++)
                // {
                //     var query = new Query1(new string('a', random.Next(1, 5)));
                //     ByteBuffer data, meta;
                //     ChannelMessage result;
                //     (meta, data) = coreSerializer.Serialize(query);
                //     result = peripheryMessageProcessor(new ChannelMessage
                //     {
                //         Data = data,
                //         MetaData = meta
                //     });
                //     var response = (Response1)coreSerializer.Deserialize(result.MetaData, result.Data);
                // }
            }

            [Benchmark]
            public void ParallelTest()
            {
                // var pipedTwoChannels = new PipedTwoChannels();
                // var coreSerializer = new ChannelDataSerializer();
                // var coreService = new Service(pipedTwoChannels.First);
                // var periphery = new Periphery.Periphery(pipedTwoChannels.Second);
                // var random = new Random();
                // var serializerLock = new object();
                // var peripheryMessageProcessor = coreService.QueryRemoteService<Func<ChannelMessage, ChannelMessage>>();
                // var forResult = Parallel.For(0, _testQueryCount, (i, state) =>
                // {
                //     var query = new Query1(new string('a', random.Next(1, 5)));
                //     ByteBuffer data, meta;
                //     ChannelMessage result = null;
                //     lock (serializerLock)
                //     {
                //         (meta, data) = coreSerializer.Serialize(query);
                //         result = peripheryMessageProcessor(new ChannelMessage
                //         {
                //             Data = data,
                //             MetaData = meta
                //         });
                //         var response = (Response1)coreSerializer.Deserialize(result.MetaData, result.Data);
                //     }
                // });
            }
        }
    }
}
