﻿using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BTDB.Buffer;
using CloudMicroServices.CloudTcp.Shared;

namespace CloudMicroServices.CloudTcp.Periphery
{
    public class PeripheryTcpServer
    {
        readonly PeripheryPayloadProcessor _payloadProcessor;
        readonly CancellationToken _token;
        readonly Socket _listenSocket;

        public PeripheryTcpServer(PeripheryPayloadProcessor payloadProcessor, CancellationToken token)
        {
            _payloadProcessor = payloadProcessor;
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
            // ReSharper disable once FunctionNeverReturns
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
                var (meta, data) = _payloadProcessor.ProcessPayload(buffer);
                if (meta != default)
                    await writer.WriteAsync(CreateResponsePayload(MessageType.Metadata, meta), _token);
                if (data != default)
                    await writer.WriteAsync(CreateResponsePayload(MessageType.Response, data), _token);
                // Tell the PipeReader how much of the buffer has been consumed.
                reader.AdvanceTo(buffer.End); // all for now, but for more 
            }
            // ReSharper disable once FunctionNeverReturns
        }

        static byte[] CreateResponsePayload(MessageType messageType, ByteBuffer messageBody)
        {
            return new PayloadBuilder()
                .SetMessageType(messageType)
                .SetMessageBuffer(messageBody)
                .Build();
        }
    }
}