﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BTDB.Service;

namespace CloudMicroServices
{
    class Program
    {
        static async Task Main()
        {
            var peripheryChannels = new PeripheryChannels();
            StartupPeripheryThread(peripheryChannels);
            var coreSerializer = new ChannelDataSerializer();
            var random = new Random();
            var numberAllocator = new NumberAllocator(default);
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
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
                Console.WriteLine($"Received response to query {query.Data} with data {response.Data}");
            }
        }

        static void StartupPeripheryThread(PeripheryChannels peripheryChannels)
        {
            var periphery = new Periphery(peripheryChannels);
            var peripheryThread = new Thread(periphery.Start)
            {
                IsBackground = true,
                Name = nameof(periphery)
            };
            peripheryThread.Start();
        }
    }
}
