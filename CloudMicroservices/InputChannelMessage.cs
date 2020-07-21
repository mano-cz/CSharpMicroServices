using CloudMicroservices.Shared;

namespace CloudMicroServices
{
    public class InputChannelMessage : ChannelMessage
    {
        public uint CorrelationId { get; set; }

        public override string ToString()
        {
            return $"ID {CorrelationId}";
        }
    }
}