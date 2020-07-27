using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CloudMicroServices.TcpClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // core
            var clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Connecting to port 8087");
            clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 8087));
            var stream = new NetworkStream(clientSocket);
            await Console.OpenStandardInput().CopyToAsync(stream);
        }
    }
}
