using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CloudMicroServices.CloudTcp.Periphery
{
    public class PeripheryTcpServer
    {
        readonly PeripheryMessageProcessor _messageProcessor;
        readonly CancellationToken _token;
        readonly Socket _listenSocket;

        public PeripheryTcpServer(PeripheryMessageProcessor messageProcessor, CancellationToken token)
        {
            _messageProcessor = messageProcessor;
            _token = token;
            _listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task ListenAsync(IPEndPoint ipEndPoint)
        {
            _listenSocket.Bind(ipEndPoint);
            _listenSocket.Listen(120);
            Console.WriteLine($"Listening on port {ipEndPoint.Port}");
            while (true)
            {
                var socket = await _listenSocket.AcceptAsync();
                _ = ProcessNewSocketAsync(socket);
            }
        }

        async Task ProcessNewSocketAsync(Socket socket)
        {
            Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");
            var stream = new NetworkStream(socket);
            var reader = PipeReader.Create(stream);
            var writer = PipeWriter.Create(stream);
            while (true)
            {
                var result = await reader.ReadAsync(_token);
                var buffer = result.Buffer;
                var responseMessage = _messageProcessor.ProcessMessage(buffer.ToArray()); // maybe not to array + response

                await writer.WriteAsync(responseMessage, _token);
                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.End);
            }
        }
    }
}