using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Running;
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
            var forResult = Parallel.For(0l, 20, (i, state) =>
            {
                var query = new Query1(new string('a', random.Next(1, 5)));
                var response = coreMessageProcessor.ProcessQuery(query, i);
                Console.WriteLine($"{i} DATA: {response.Data}");
            });
        }

        [MemoryDiagnoser]
        [RankColumn(NumeralSystem.Stars)]
        [JsonExporterAttribute.Full]
        public class SeriesVsParallelCore
        {
            readonly int _testQueryCount = 10;

            [Benchmark]
            public void ParallelTest()
            {
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
                var forResult = Parallel.For(0l, 20, (i, state) =>
                {
                    var query = new Query1(new string('a', random.Next(1, 5)));
                    var response = coreMessageProcessor.ProcessQuery(query, i);
                    Console.WriteLine($"{i} DATA: {response.Data}");
                });
            }
        }
    }
}
