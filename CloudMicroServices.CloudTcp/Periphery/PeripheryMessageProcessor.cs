using System;

namespace CloudMicroServices.CloudTcp.Periphery
{
    public class PeripheryMessageProcessor
    {
        public byte[] ProcessMessage(byte[] message)
        {
            Console.WriteLine($"Received {message[0]}");
            return new byte[1] { (byte)(message[0] * 2) };
        }
    }
}