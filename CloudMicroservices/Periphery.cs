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
            var serializer = new ChannelDataSerializer();
            while (true)
            {
                var t = _peripheryChannels.Input.Reader.ReadAsync();
                var nextMessage = t.AsTask().Result;
                var nextQuery = (Query1)serializer.Deserialize(nextMessage.MetaData, nextMessage.Data);
                Console.WriteLine($"Received next message: {nextQuery.Data}");
                if (!_peripheryChannels.Output.TryGetValue(nextMessage.CorrelationId, out var outputChannel))
                    throw new InvalidOperationException();
                var response = new Response1 { Data = nextQuery.Data + "X" };
                var (meta, data) = serializer.Serialize(response);
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