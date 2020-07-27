using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CloudMicroServices.CloudTcp.Core;
using CloudMicroServices.CloudTcp.Periphery;

namespace CloudMicroServices.CloudTcp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var peripheryThread = new Thread(() =>
            {
                try
                {
                    var peripheryTcpServer =
                        new PeripheryTcpServer(new PeripheryMessageProcessor(), cancellationTokenSource.Token);
                    peripheryTcpServer.ListenAsync(new IPEndPoint(IPAddress.Loopback, 8087))
                        .Wait(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException e)
                {
                    // canceled
                }
            })
            {
                IsBackground = true,
                Name = "Periphery"
            };
            peripheryThread.Start();

            Parallel.For(1, 4, (i, state) =>
            {
                // socket allocation per query, should be pool, locking etc.
                var peripheryClient = new PeripheryTcpClient();
                peripheryClient.Connect(new IPEndPoint(IPAddress.Loopback, 8087));
                var response = peripheryClient.SendAsync(new byte[1] { (byte)i }).Result;
                // var response = await peripheryClient.SendAsync(new byte[1] { (byte)i });
                // Console.WriteLine($"Response Len {response.Length}");
                Console.WriteLine($"Response {i}={response[0]}");
                // await Task.Delay(1000);
            });
            cancellationTokenSource.Cancel();
        }
    }
}
