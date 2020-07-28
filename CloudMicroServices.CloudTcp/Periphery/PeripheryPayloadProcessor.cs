using System.Buffers;
using CloudMicroServices.CloudTcp.Shared;

namespace CloudMicroServices.CloudTcp.Periphery
{
    public class PeripheryPayloadProcessor
    {
        public byte[] ProcessPayload(ReadOnlySequence<byte> payloadSequence)
        {
            var payloadReader = new PayloadReader(payloadSequence);
            // Console.WriteLine($"Received {message[0]}");
            // return new byte[1] { (byte)(message[0] * 2) };
            return default;
        }
    }
}