using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CloudMicroServices.CloudTcp.Core
{
    public class PeripheryTcpClient : IAsyncDisposable
    {
        readonly Socket _clientSocket;
        NetworkStream _stream;
        PipeReader _reader;
        PipeWriter _writer;

        public PeripheryTcpClient()
        {
            _clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(IPEndPoint ipEndPoint)
        {
            _clientSocket.Connect(ipEndPoint);
            Console.WriteLine($"Connecting to port {ipEndPoint}");
            _stream = new NetworkStream(_clientSocket);
            _reader = PipeReader.Create(_stream);
            _writer = PipeWriter.Create(_stream);
        }

        public async ValueTask DisposeAsync()
        {
            _clientSocket?.Dispose();
            if (_stream != null)
                await _stream.DisposeAsync();
        }

        public async ValueTask<byte[]> SendAsync(byte[] payload)
        {
            // todo read article for better impl.
            await _writer.WriteAsync(payload);
            var response = await _reader.ReadAsync();
            var buffer = response.Buffer;
            var responseBytes = buffer.ToArray(); // can be avoided, it can ready it via segmets foreach without allocation, this allocates
            // maybe loop if i don't have have full response
            _reader.AdvanceTo(buffer.End);
            return responseBytes;
        }

        public async Task SendWithoutResponseAsync(byte[] payload)
        {
            await _writer.WriteAsync(payload);
        }
    }
}