using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using BTDB.Buffer;
using CloudMicroServices.CloudTcp.Shared;

namespace CloudMicroServices.CloudTcp.Core
{
    public class PeripheryTcpClient : IAsyncDisposable
    {
        readonly CorePayloadProcessor _corePayloadProcessor;
        readonly Socket _clientSocket;
        NetworkStream _stream;
        PipeReader _reader;
        PipeWriter _writer;
        readonly object _serializationLock = new object();

        public PeripheryTcpClient(CorePayloadProcessor corePayloadProcessor)
        {
            _corePayloadProcessor = corePayloadProcessor;
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

        public async ValueTask<object> SendAsync(object obj)
        {
            ByteBuffer data;
            lock (_serializationLock)
            {
                ByteBuffer meta;
                (meta, data) = _corePayloadProcessor.PreparePayload(obj);
                if (meta != default)
                {
                    var metaPayload = new PayloadBuilder()
                        .SetMessageType(MessageType.Metadata)
                        .SetMessageBuffer(meta)
                        .Build();
                    _writer.WriteAsync(metaPayload).AsTask().Wait();
                }
            }
            var dataPayload = new PayloadBuilder()
                .SetMessageType(MessageType.Query)
                .SetMessageBuffer(data)
                .Build();
            await _writer.WriteAsync(dataPayload);
            var response = await _reader.ReadAsync();
            var buffer = response.Buffer;
            var responseObj = _corePayloadProcessor.ProcessPayload(buffer);
            // maybe loop if i don't have have full response
            _reader.AdvanceTo(buffer.End);
            if (responseObj == default)
            {
                response = await _reader.ReadAsync();
                buffer = response.Buffer;
                responseObj = _corePayloadProcessor.ProcessPayload(buffer);
                if (responseObj == default)
                    throw new InvalidOperationException();
                _reader.AdvanceTo(buffer.End);
            }
            return responseObj;
        }

        public async ValueTask DisposeAsync()
        {
            _clientSocket?.Dispose();
            if (_stream != null)
                await _stream.DisposeAsync();
        }
    }
}