using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CloudMicroServices.Tcp;

namespace CloudMicroServices.TcpClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // core
            var cancellationTokenSource = new CancellationTokenSource();
            var peripheryThread = new Thread(() =>
            {
                var peripheryTcpServer = new PeripheryTcpServer(new PeripheryMessageProcessor());
                peripheryTcpServer.ListenAsync(new IPEndPoint(IPAddress.Loopback, 8087)).Wait(cancellationTokenSource.Token);
            })
            {
                IsBackground = true,
                Name = "Periphery"
            };
            peripheryThread.Start();

            Parallel.For(1, 4, (i, state) =>
            {
                var peripheryClient = new PeripheryTcpClient();
                // await using var peripheryClient = new PeripheryTcpClient();
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
