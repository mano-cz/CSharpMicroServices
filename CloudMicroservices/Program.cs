using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CloudMicroServices
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var random = new Random();
            var peripheryChannels = new PeripheryChannels();
            var periphery = new Periphery(peripheryChannels);
            var peripheryThread = new Thread(periphery.Start)
            {
                IsBackground = true,
                Name = nameof(periphery)
            };
            peripheryThread.Start();

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                var query = new Query1(new string('a', random.Next(1, 5)));
                var correlationId = (ulong)random.Next();
                var outputChannel = peripheryChannels.Output.AddOrUpdate(
                    correlationId,
                    key => Channel.CreateBounded<IResponse>(1),
                    (key, channel) => Channel.CreateBounded<IResponse>(1));
                await peripheryChannels.Input.Writer.WriteAsync(new PeripheryChannelMessage
                {
                    CorrelationId = correlationId,
                    Query = query
                });
                var response = await outputChannel.Reader.ReadAsync();

                peripheryChannels.Output.Remove(correlationId, out outputChannel);
                // outputChannel.Writer.Complete();

                Console.WriteLine($"Received response to query {query} with data {response}");
            }
        }
    }
}
