﻿using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudMicroServices.Tcp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // periphery server
            var server = new PeripheryTcpServer(new PeripheryMessageProcessor(), new CancellationToken());
            await server.ListenAsync(new IPEndPoint(IPAddress.Loopback, 8087));

            // var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            // listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 8087));
            // Console.WriteLine("Listening on port 8087");
            // listenSocket.ListenAsync(120); // 120 connections can be queued for acceptance, does not block
            // while (true)
            // {
            //     var socket = await listenSocket.AcceptAsync();
            //     _ = ProcessLinesAsync(socket); // do not block on new connection
            // }
        }

        static async Task ProcessLinesAsync(Socket socket)
        {
            Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");
            var stream = new NetworkStream(socket);
            var reader = PipeReader.Create(stream);
            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    ProcessLine(line);
                }

                // Tell the PipeReader how much of the buffer has been consumed.
                // nutno volat i v prip. ze nenajdu ... dostanu priste to sami + more, jinak kdyz neco zkonzumuju tak pristi read vrati az navazujici
                reader.AdvanceTo(buffer.Start, buffer.End);// (pozice pred kerou jsem vse zkonzumoval, pozice pred kterou jsem vse videl)

                // Stop reading if there's no more data coming.
                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Mark the PipeReader as complete.
            await reader.CompleteAsync();

            Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
        }

        static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            // Look for a EOL in the buffer.
            SequencePosition? position = buffer.PositionOf((byte)'\n');

            if (position == null)
            {
                line = default;
                return false;
            }

            // Skip the line + the \n.
            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }

        static void ProcessLine(in ReadOnlySequence<byte> buffer)
        {
            foreach (var segment in buffer)
                Console.Write(Encoding.UTF8.GetString(segment.Span));
            Console.WriteLine();
        }
    }
}
