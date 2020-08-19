using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CloudMicroServices.CloudTcp.Core;
using CloudMicroServices.CloudTcp.Periphery;
using CloudMicroServices.CloudTcp.Shared;

namespace CloudMicroServices.CloudTcp
{
    class Program
    {
        static void Main()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            StartPeriphery(cancellationTokenSource);
            Parallel.For(1, 2, async (i, state) =>
            {
                // socket allocation per query, should be pool, locking etc.
                await using var peripheryClient = new PeripheryTcpClient(new CorePayloadProcessor(new MessageSerializer()));
                peripheryClient.Connect(new IPEndPoint(IPAddress.Loopback, 8087));
                var response = (Response1)peripheryClient.SendAsync(new Query1 { Data = "a" }).Result;
                Console.WriteLine($"Response processed `{response.Data}`.");
            });
            cancellationTokenSource.Cancel();
        }

        static void StartPeriphery(CancellationTokenSource cancellationTokenSource)
        {
            var peripheryThread = new Thread(() =>
            {
                try
                {
                    var peripheryTcpServer =
                        new PeripheryTcpServer(new PeripheryPayloadProcessor(new MessageSerializer()), cancellationTokenSource.Token);
                    peripheryTcpServer.ListenAsync(new IPEndPoint(IPAddress.Loopback, 8087))
                        .Wait(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException e)
                {
                    // canceled
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            })
            {
                IsBackground = true,
                Name = "Periphery"
            };
            peripheryThread.Start();
        }
    }
}
