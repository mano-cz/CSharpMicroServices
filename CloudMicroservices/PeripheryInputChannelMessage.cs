namespace CloudMicroServices
{
    public class PeripheryInputChannelMessage : PeripheryChannelMessage
    {
        public uint CorrelationId { get; set; }

        public override string ToString()
        {
            return $"ID {CorrelationId}";
        }
    }
}