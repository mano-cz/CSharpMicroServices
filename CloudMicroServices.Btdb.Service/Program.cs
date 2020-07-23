using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Running;
using BTDB.Buffer;
using BTDB.Service;
using CloudMicroServices.Btdb.Rx.Periphery;
using CloudMicroservices.Shared;

namespace CloudMicroServices.Btdb.Rx.Core
{
    public class Program
    {
        public static void Main()
        {
            // var summary = BenchmarkRunner.Run<SeriesVsParallelCore>();
            // registrations
            var pipedTwoChannels = new PipedTwoChannels();
            var coreMessageProcessor = new CoreMessageProcessor();
            var coreService = new Service(pipedTwoChannels.Second);
            var peripheryService = new Service(pipedTwoChannels.First);
            coreService.RegisterLocalService<IMessageProcessor>(coreMessageProcessor);
            var peripheryMessageProcessor = new PeripheryMessageProcessor();
            peripheryService.RegisterLocalService<IMessageProcessor>(peripheryMessageProcessor);
            coreMessageProcessor.Initialize(coreService.QueryRemoteService<IMessageProcessor>());
            peripheryMessageProcessor.Initialize(peripheryService.QueryRemoteService<IMessageProcessor>());

            // processing
            var random = new Random();
            var serializerLock = new object();
            var forResult = Parallel.For(0l, 5, (i, state) =>
            {
                var query = new Query1(new string('a', random.Next(1, 5)));
                Console.WriteLine($"{i} QUERY: {query.Data}");
                byte[] data, meta;

                lock (serializerLock)
                {
                    Console.WriteLine($"{i} Start serialization");
                    (meta, data) = coreMessageProcessor.Serialize(query);
                    if (meta != default)
                    {
                        Console.WriteLine($"{i} Has new meta");
                        peripheryMessageProcessor.ProcessMetadata(meta);
                    }
                    Console.WriteLine($"{i} End serialization");
                }
                Console.WriteLine($"{i} Deserialization START");
                try
                {
                    var responseData = peripheryMessageProcessor.ProcessData(data);
                    // lock (serializerLock)
                    // {
                    var response = (Response1)coreMessageProcessor.Deserialize(responseData);
                    // }
                    //
                    Console.WriteLine($"{i} DATA: {response.Data}");
                }
                catch (Exception e)
                {
                    // why buffer new is needed
                    Console.WriteLine(e);
                    throw;
                }
                Console.WriteLine($"{i} Deserialization END");
            });
        }

        [MemoryDiagnoser]
        [RankColumn(NumeralSystem.Stars)]
        [JsonExporterAttribute.Full]
        public class SeriesVsParallelCore
        {
            readonly int _testQueryCount = 10;

            // [Benchmark]
            // public void SeriesTest()
            // {
            //     var pipedTwoChannels = new PipedTwoChannels();
            //     var coreSerializer = new ChannelDataSerializer();
            //     var coreService = new Service(pipedTwoChannels.First);
            //     var periphery = new Periphery.Periphery(pipedTwoChannels.Second);
            //     var random = new Random();
            //     var peripheryMessageProcessor = coreService.QueryRemoteService<Func<PeripheryChannelMessageRx, PeripheryChannelMessageRx>>();
            //     for (int i = 0; i < _testQueryCount; i++)
            //     {
            //         var query = new Query1(new string('a', random.Next(1, 5)));
            //         ByteBuffer data, meta;
            //         PeripheryChannelMessageRx result;
            //         (meta, data) = coreSerializer.Serialize(query);
            //         result = peripheryMessageProcessor(new PeripheryChannelMessageRx
            //         {
            //             Data = data,
            //             MetaData = meta
            //         });
            //         var response = (Response1)coreSerializer.Deserialize(result.MetaData, result.Data);
            //     }
            // }

            // [Benchmark]
            // public void ParallelTest()
            // {
            //     var pipedTwoChannels = new PipedTwoChannels();
            //     var coreSerializer = new ChannelDataSerializer();
            //     var coreService = new Service(pipedTwoChannels.First);
            //     var periphery = new Periphery.Periphery(pipedTwoChannels.Second);
            //     var random = new Random();
            //     var serializerLock = new object();
            //     var peripheryMessageProcessor = coreService.QueryRemoteService<Func<PeripheryChannelMessageRx, PeripheryChannelMessageRx>>();
            //     var forResult = Parallel.For(0, _testQueryCount, (i, state) =>
            //     {
            //         var query = new Query1(new string('a', random.Next(1, 5)));
            //         ByteBuffer data, meta;
            //         PeripheryChannelMessageRx result = null;
            //         lock (serializerLock)
            //         {
            //             (meta, data) = coreSerializer.Serialize(query);
            //             result = peripheryMessageProcessor(new PeripheryChannelMessageRx
            //             {
            //                 Data = data,
            //                 MetaData = meta
            //             });
            //             var response = (Response1)coreSerializer.Deserialize(result.MetaData, result.Data);
            //         }
            //     });
            // }

            [Benchmark]
            public void ParallelTestNewGeneration()
            {
                // var pipedTwoChannels = new PipedTwoChannels();
                // var cService = new Service(pipedTwoChannels.First);
                // var pService = new Service(pipedTwoChannels.Second);
                // var coreService = new CoreMessageProcessor(() => pService.QueryRemoteService<IMessageProcessor>());
                // var peripheryService = new PeripheryService(() => cService.QueryRemoteService<IMessageProcessor>());
                // pService.RegisterLocalService(peripheryService);
                // cService.RegisterLocalService(coreService);
                //
                // var random = new Random();
                // var serializerLock = new object();
                // var forResult = Parallel.For(0l, _testQueryCount, (i, state) =>
                // {
                //     var query = new Query1(new string('a', random.Next(1, 5)));
                //     ByteBuffer data, meta;
                //     PeripheryChannelMessageRx result = null;
                //     // coreService.ProcessData()
                //
                //     lock (serializerLock)
                //     {
                //         (meta, data) = coreService.Serialize(query);
                //         if (meta != default)
                //             peripheryService.ProcessMetadata(meta);
                //     }
                //     var responseData = peripheryService.ProcessData(data);
                //     lock (serializerLock)
                //     {
                //         var response = coreService.Deserialize<Response1>(responseData);
                //     }
                // });
            }
        }
    }
}
