using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using BTDB.Buffer;
using BTDB.Service;
using CloudMicroservices.Shared;

namespace CloudMicroServices
{
    public class Program
    {
        public static void Main()
        {
            // var summary = BenchmarkRunner.Run<SeriesVsParallelCore>();
            var peripheryChannels = new PeripheryChannels();
            var cancellationTokenSource = new CancellationTokenSource();
            var periphery = new Periphery(peripheryChannels, cancellationTokenSource.Token);
            var peripheryThread = new Thread(periphery.Start)
            {
                IsBackground = true,
                Name = nameof(periphery)
            };
            peripheryThread.Start();
            var coreSerializer = new ChannelDataSerializer2();
            var random = new Random();
            var numberAllocator = new NumberAllocator(default);
            var coreLock = new object();
            var result = Parallel.For((long)0, 2, (i, state) => // 10, 10 zmena na 1, 2
            {
                Console.WriteLine($"Thread {i}");
                var query = new Query1(new string('a', random.Next(1, 5)));
                var correlationId = numberAllocator.Allocate();
                var outputChannel = peripheryChannels.Output.AddOrUpdate(
                    correlationId,
                    key => PeripheryChannels.CreateOutputChannel(),
                    (key, channel) => PeripheryChannels.CreateOutputChannel());
                ByteBuffer data;
                lock (coreLock)
                {
                    Console.WriteLine($"{i} Query Serialization");
                    ByteBuffer meta;
                    (meta, data) = coreSerializer.Serialize(query);
                    if (meta != default)
                    {
                        Console.WriteLine($"{i} New Meta - Query Meta");
                        peripheryChannels.Input.Writer.WriteAsync(new InputChannelMessage
                        {
                            CorrelationId = correlationId,
                            Payload = meta,
                            Type = ChannelMessagePayloadType.Metadata
                        }, cancellationTokenSource.Token).AsTask().Wait(cancellationTokenSource.Token);
                        Console.WriteLine($"{i} Waiting for New Meta Response");
                        var responseMessage2 = outputChannel.Reader.ReadAsync(cancellationTokenSource.Token).AsTask().Result;
                        if (responseMessage2.Type != ChannelMessagePayloadType.MetadataProcessed)
                            throw new InvalidOperationException("Type should be MetadataProcessed.");
                        Console.WriteLine($"{i} Query New Meta Response received");
                    }
                }
                Console.WriteLine($"{i} Sending data from core");
                peripheryChannels.Input.Writer.WriteAsync(new InputChannelMessage
                {
                    CorrelationId = correlationId,
                    Payload = data,
                    Type = ChannelMessagePayloadType.Data
                }, cancellationTokenSource.Token).AsTask().Wait(cancellationTokenSource.Token);
                Console.WriteLine($"{i} Waiting for response from periphery");
                var responseMessage = outputChannel.Reader.ReadAsync(cancellationTokenSource.Token).AsTask().Result;
                lock (coreLock) // UNCOMMENT
                {
                    Console.WriteLine($"{i} Periphery response received");
                    if (responseMessage.Type == ChannelMessagePayloadType.Metadata)
                    {
                        Console.WriteLine($"{i} It was METADATA Periphery response, gonna process");
                        coreSerializer.ProcessDeserializerMetadata(responseMessage.Payload);

                        // write meta confirmation id
                        peripheryChannels.InputMetadataConfirmation.Writer.WriteAsync(correlationId, cancellationTokenSource.Token).AsTask().Wait(cancellationTokenSource.Token);

                        Console.WriteLine($"{i} Response data wait");
                        responseMessage = outputChannel.Reader.ReadAsync(cancellationTokenSource.Token).AsTask().Result;
                        Console.WriteLine($"{i} Response data received");
                    }
                    else
                    {
                        Console.WriteLine($"{i} It was DATA Periphery response");
                    }
                }
                if (responseMessage.Type != ChannelMessagePayloadType.Data)
                    throw new InvalidOperationException("Type should be Data.");
                Console.WriteLine($"{i} FINAL Deserialize");
                var response = (Response1)coreSerializer.Deserialize(responseMessage.Payload);
                peripheryChannels.Output.Remove(correlationId, out outputChannel);
                numberAllocator.Deallocate(correlationId);

            });
            cancellationTokenSource.Cancel();
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
            // var peripheryChannels = new PeripheryChannels();
            // var cancellationTokenSource = new CancellationTokenSource();
            // var periphery = new Periphery(peripheryChannels, cancellationTokenSource.Token);
            // var peripheryThread = new Thread(periphery.Start)
            // {
            //     IsBackground = true,
            //     Name = nameof(periphery)
            // };
            // peripheryThread.Start();
            //
            // var coreSerializer = new ChannelDataSerializer();
            // var random = new Random();
            // var numberAllocator = new NumberAllocator(default);
            // for (int i = 0; i < _testQueryCount; i++)
            // {
            //     var query = new Query1(new string('a', random.Next(1, 5)));
            //     var correlationId = numberAllocator.Allocate();
            //     var outputChannel = peripheryChannels.Output.AddOrUpdate(
            //         correlationId,
            //         key => PeripheryChannels.CreateOutputChannel(),
            //         (key, channel) => PeripheryChannels.CreateOutputChannel());
            //     var (meta, data) = coreSerializer.Serialize(query);
            //     await peripheryChannels.Input.Writer.WriteAsync(new InputChannelMessage
            //     {
            //         CorrelationId = correlationId,
            //         Data = data,
            //         MetaData = meta // maybe...could be separate, because often empty
            //     });
            //     var responseMessage = await outputChannel.Reader.ReadAsync();
            //     var response = (Response1)coreSerializer.Deserialize(responseMessage.MetaData, responseMessage.Data);
            //     peripheryChannels.Output.Remove(correlationId, out outputChannel);
            //     numberAllocator.Deallocate(correlationId);
            // }
            // cancellationTokenSource.Cancel();
        }

        [Benchmark]
        public void ParallelTest()
        {
            // var peripheryChannels = new PeripheryChannels();
            // var cancellationTokenSource = new CancellationTokenSource();
            // var periphery = new Periphery(peripheryChannels, cancellationTokenSource.Token);
            // var peripheryThread = new Thread(periphery.Start)
            // {
            //     IsBackground = true,
            //     Name = nameof(periphery)
            // };
            // peripheryThread.Start();
            //
            // var coreSerializer = new ChannelDataSerializer();
            // var random = new Random();
            // var numberAllocator = new NumberAllocator(default);
            // var serializerLock = new object();
            // var result = Parallel.For(0, _testQueryCount, (i, state) =>
            // {
            //     var query = new Query1(new string('a', random.Next(1, 5)));
            //     var correlationId = numberAllocator.Allocate();
            //     var outputChannel = peripheryChannels.Output.AddOrUpdate(
            //         correlationId,
            //         key => PeripheryChannels.CreateOutputChannel(),
            //         (key, channel) => PeripheryChannels.CreateOutputChannel());
            //     lock (serializerLock)
            //     {
            //         ByteBuffer data, meta;
            //         (meta, data) = coreSerializer.Serialize(query);
            //         peripheryChannels.Input.Writer.WriteAsync(new InputChannelMessage
            //         {
            //             CorrelationId = correlationId,
            //             Data = data,
            //             MetaData = meta // maybe...could be separate, because often empty
            //         }).AsTask().Wait();
            //
            //         var responseMessage = outputChannel.Reader.ReadAsync().AsTask().Result;
            //         var response = (Response1)coreSerializer.Deserialize(responseMessage.MetaData, responseMessage.Data);
            //     }
            //
            //     peripheryChannels.Output.Remove(correlationId, out outputChannel);
            //     numberAllocator.Deallocate(correlationId);
            // });
            // cancellationTokenSource.Cancel();
        }
    }
}