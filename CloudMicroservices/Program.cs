using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Running;
using BTDB.Buffer;
using BTDB.Service;
using CloudMicroservices.Shared;

namespace CloudMicroServices
{
    class Program
    {
        static async Task Main()
        {
            var summary = BenchmarkRunner.Run<SeriesVsParallelCore>();
        }
    }

    [MemoryDiagnoser]
    [RankColumn(NumeralSystem.Stars)]
    [JsonExporterAttribute.Full]
    public class SeriesVsParallelCore
    {
        readonly int _testQueryCount = 10;

        [Benchmark]
        public async Task SeriesTest()
        {
            var peripheryChannels = new PeripheryChannels();
            var cancellationTokenSource = new CancellationTokenSource();
            var periphery = new Periphery(peripheryChannels, cancellationTokenSource.Token);
            var peripheryThread = new Thread(periphery.Start)
            {
                IsBackground = true,
                Name = nameof(periphery)
            };
            peripheryThread.Start();

            var coreSerializer = new ChannelDataSerializer();
            var random = new Random();
            var numberAllocator = new NumberAllocator(default);
            for (int i = 0; i < _testQueryCount; i++)
            {
                var query = new Query1(new string('a', random.Next(1, 5)));
                var correlationId = numberAllocator.Allocate();
                var outputChannel = peripheryChannels.Output.AddOrUpdate(
                    correlationId,
                    key => PeripheryChannels.CreateBoundedOutputChannel(),
                    (key, channel) => PeripheryChannels.CreateBoundedOutputChannel());
                var (meta, data) = coreSerializer.Serialize(query);
                await peripheryChannels.Input.Writer.WriteAsync(new PeripheryInputChannelMessage
                {
                    CorrelationId = correlationId,
                    Data = data,
                    MetaData = meta // maybe...could be separate, because often empty
                });
                var responseMessage = await outputChannel.Reader.ReadAsync();
                var response = (Response1)coreSerializer.Deserialize(responseMessage.MetaData, responseMessage.Data);
                peripheryChannels.Output.Remove(correlationId, out outputChannel);
                numberAllocator.Deallocate(correlationId);
            }
            cancellationTokenSource.Cancel();
        }

        [Benchmark]
        public void ParallelTest()
        {
            var peripheryChannels = new PeripheryChannels();
            var cancellationTokenSource = new CancellationTokenSource();
            var periphery = new Periphery(peripheryChannels, cancellationTokenSource.Token);
            var peripheryThread = new Thread(periphery.Start)
            {
                IsBackground = true,
                Name = nameof(periphery)
            };
            peripheryThread.Start();

            var coreSerializer = new ChannelDataSerializer();
            var random = new Random();
            var numberAllocator = new NumberAllocator(default);
            var serializerLock = new object();
            var result = Parallel.For(0, _testQueryCount, (i, state) =>
            {
                var query = new Query1(new string('a', random.Next(1, 5)));
                var correlationId = numberAllocator.Allocate();
                var outputChannel = peripheryChannels.Output.AddOrUpdate(
                    correlationId,
                    key => PeripheryChannels.CreateBoundedOutputChannel(),
                    (key, channel) => PeripheryChannels.CreateBoundedOutputChannel());
                lock (serializerLock)
                {
                    ByteBuffer data, meta;
                    (meta, data) = coreSerializer.Serialize(query);
                    peripheryChannels.Input.Writer.WriteAsync(new PeripheryInputChannelMessage
                    {
                        CorrelationId = correlationId,
                        Data = data,
                        MetaData = meta // maybe...could be separate, because often empty
                    }).AsTask().Wait();

                    var responseMessage = outputChannel.Reader.ReadAsync().AsTask().Result;
                    var response = (Response1)coreSerializer.Deserialize(responseMessage.MetaData, responseMessage.Data);
                }

                peripheryChannels.Output.Remove(correlationId, out outputChannel);
                numberAllocator.Deallocate(correlationId);
            });
            cancellationTokenSource.Cancel();
        }
    }
}