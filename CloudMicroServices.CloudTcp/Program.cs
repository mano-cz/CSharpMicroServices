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
            var serializer = new MessageSerializer();
            var serializationLock = new object();
            Parallel.For(1, 2, (i, state) =>
            {
                // socket allocation per query, should be pool, locking etc.
                var peripheryClient = new PeripheryTcpClient();
                peripheryClient.Connect(new IPEndPoint(IPAddress.Loopback, 8087));
                lock (serializationLock)
                {
                    var (meta, data) = serializer.Serialize(new Query1 { Data = "a" });
                    if (meta != default)
                    {
                        var metaPayload = new PayloadBuilder()
                            .SetMessageType(MessageType.Metadata)
                            .SetMessageBuffer(meta)
                            .Build();
                        peripheryClient.SendWithoutResponseAsync(metaPayload).Wait(cancellationTokenSource.Token);
                    }
                }
                // var response = peripheryClient.SendAsync(message).Result;
                // var response = await peripheryClient.SendAsync(new byte[1] { (byte)i });
                // Console.WriteLine($"Response Len {response.Length}");
                // Console.WriteLine($"Response {i}={response[0]}");
                // await Task.Delay(1000);
            });
            while (true) { }
            // Task.Delay(1000).ContinueWith(t =>
            // {
            //     cancellationTokenSource.Cancel();
            // });
        }

        static void StartPeriphery(CancellationTokenSource cancellationTokenSource)
        {
            var peripheryThread = new Thread(() =>
            {
                try
                {
                    var peripheryTcpServer =
                        new PeripheryTcpServer(new PeripheryPayloadProcessor(), cancellationTokenSource.Token);
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
