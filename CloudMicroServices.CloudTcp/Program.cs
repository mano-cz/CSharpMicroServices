using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BTDB.Buffer;
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
                var peripheryClient = new PeripheryTcpClient(new CorePayloadProcessor(new MessageProcessor(new MessageSerializer())));
                peripheryClient.Connect(new IPEndPoint(IPAddress.Loopback, 8087));
                ByteBuffer data;
                lock (serializationLock)
                {
                    ByteBuffer meta;
                    (meta, data) = serializer.Serialize(new Query1 { Data = "a" });
                    if (meta != default)
                    {
                        var metaPayload = new PayloadBuilder()
                            .SetMessageType(MessageType.Metadata)
                            .SetMessageBuffer(meta)
                            .Build();
                        peripheryClient.SendWithoutResponseAsync(metaPayload).Wait(cancellationTokenSource.Token);
                    }
                }
                var dataPayload = new PayloadBuilder()
                    .SetMessageType(MessageType.Query)
                    .SetMessageBuffer(data)
                    .Build();
                peripheryClient.SendAsync(dataPayload).AsTask().Wait(cancellationTokenSource.Token);
            });
            while (true) { }
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
