using System;
using System.Threading;
using System.Threading.Tasks;
using BTDB.Buffer;
using CloudMicroservices.Shared;

namespace CloudMicroServices
{
    public class Periphery
    {
        readonly PeripheryChannels _peripheryChannels;
        readonly CancellationToken _token;

        public Periphery(PeripheryChannels peripheryChannels, CancellationToken token)
        {
            _peripheryChannels = peripheryChannels;
            _token = token;
        }

        public void Start()
        {
            try
            {
                var peripherySerializer = new ChannelDataSerializer2();
                while (true)
                {
                    var nextMessage = _peripheryChannels.Input.Reader.ReadAsync(_token).AsTask().Result;
                    if (!_peripheryChannels.Output.TryGetValue(nextMessage.CorrelationId, out var outputChannel))
                        throw new InvalidOperationException();
                    if (nextMessage.Type == ChannelMessagePayloadType.Metadata)
                    {
                        Console.WriteLine("PERIPHERY: META MSG");
                        peripherySerializer.ProcessDeserializerMetadata(nextMessage.Payload);
                        outputChannel.Writer.WriteAsync(
                                new ChannelMessage { Type = ChannelMessagePayloadType.MetadataProcessed }, _token)
                            .AsTask().Wait(_token);
                        continue;
                    }
                    Console.WriteLine("PERIPHERY: DATA MSG");
                    var nextQuery = (Query1)peripherySerializer.Deserialize(nextMessage.Payload);
                    Console.WriteLine($"Received next message: {nextQuery.Data}");
                    var response = new Response1 { Data = $"{nextQuery.Data}Response" };
                    ByteBuffer data, meta;
                    (meta, data) = peripherySerializer.Serialize(response);
                    if (meta != default)
                    {
                        outputChannel.Writer.WriteAsync(
                        new ChannelMessage
                        {
                            Type = ChannelMessagePayloadType.Metadata,
                            Payload = meta
                        }, _token
                        ).AsTask().Wait(_token);

                        Console.WriteLine("Waiting for New Meta Response From Core");
                        var confirmationId = _peripheryChannels.InputMetadataConfirmation.Reader.ReadAsync(_token).AsTask().Result;
                        if (confirmationId != nextMessage.CorrelationId)
                            throw new InvalidOperationException("Confirmation id should be same as correlation id.");
                        Console.WriteLine("Received New Meta Response From Core");
                    }
                    outputChannel.Writer.WriteAsync(
                        new ChannelMessage
                        {
                            Type = ChannelMessagePayloadType.Data,
                            Payload = data
                        }, _token
                    ).AsTask().Wait(_token);
                }
            }
            catch (AggregateException e) when (e.InnerException is TaskCanceledException)
            {
                // aggregate with inner inside, cancelled
            }
        }
    }
}