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
            while (true)
            {
                var t = _peripheryChannels.Input.Reader.ReadAsync();
                var nextMessage = t.AsTask().Result;
                Console.WriteLine($"Received next message: {nextMessage}");
                if (!_peripheryChannels.Output.TryGetValue(nextMessage.CorrelationId, out var outputChannel))
                    throw new InvalidOperationException();
                outputChannel.Writer.WriteAsync(new Response1 { Data = nextMessage.Query.Data + "X" }).AsTask().Wait();
            }
        }
    }
}