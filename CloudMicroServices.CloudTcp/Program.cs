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
        static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            StartPeriphery(cancellationTokenSource);
            Parallel.For(1, 2, (i, state) =>
            {
                // socket allocation per query, should be pool, locking etc.
                var peripheryClient = new PeripheryTcpClient(new CorePayloadProcessor(new MessageProcessor(new MessageSerializer())));
                peripheryClient.Connect(new IPEndPoint(IPAddress.Loopback, 8087));
                peripheryClient.SendAsync(new Query1 { Data = "a" }).AsTask().Wait(cancellationTokenSource.Token);
            });
        }

        static void StartPeriphery(CancellationTokenSource cancellationTokenSource)
        {
            var peripheryThread = new Thread(() =>
            {
                try
                {
                    var peripheryTcpServer =
                        new PeripheryTcpServer(new PeripheryPayloadProcessor(new MessageProcessor(new MessageSerializer())), cancellationTokenSource.Token);
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
