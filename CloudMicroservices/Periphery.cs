using System;
using System.Threading;
using System.Threading.Tasks;
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
                var peripherySerializer = new ChannelDataSerializer();
                while (true)
                {
                    var nextMessage = _peripheryChannels.Input.Reader.ReadAsync(_token).AsTask().Result;
                    var nextQuery = (Query1)peripherySerializer.Deserialize(nextMessage.MetaData, nextMessage.Data);
                    // Console.WriteLine($"Received next message: {nextQuery.Data}");
                    if (!_peripheryChannels.Output.TryGetValue(nextMessage.CorrelationId, out var outputChannel))
                        throw new InvalidOperationException();
                    var response = new Response1 { Data = $"{nextQuery.Data}Response" };
                    var (meta, data) = peripherySerializer.Serialize(response);
                    outputChannel.Writer.WriteAsync(
                        new PeripheryChannelMessage
                        {
                            MetaData = meta,
                            Data = data
                        }, _token
                    ).AsTask().Wait(_token);
                }
            }
            catch (Exception)
            {
                // aggregate with inner inside, cancelled
            }
        }
    }
}