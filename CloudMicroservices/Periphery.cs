using System;

namespace CloudMicroServices
{
    public class Periphery
    {
        readonly PeripheryChannels _peripheryChannels;

        public Periphery(PeripheryChannels peripheryChannels)
        {
            _peripheryChannels = peripheryChannels;
        }

        public void Start()
        {
            var peripherySerializer = new ChannelDataSerializer();
            while (true)
            {
                var nextMessage = _peripheryChannels.Input.Reader.ReadAsync().AsTask().Result;
                var nextQuery = (Query1)peripherySerializer.Deserialize(nextMessage.MetaData, nextMessage.Data);
                Console.WriteLine($"Received next message: {nextQuery.Data}");
                if (!_peripheryChannels.Output.TryGetValue(nextMessage.CorrelationId, out var outputChannel))
                    throw new InvalidOperationException();
                var response = new Response1 { Data = $"{nextQuery.Data}Response" };
                var (meta, data) = peripherySerializer.Serialize(response);
                outputChannel.Writer.WriteAsync(
                    new PeripheryChannelMessage
                    {
                        MetaData = meta,
                        Data = data
                    }
                    ).AsTask().Wait();
            }
        }
    }
}