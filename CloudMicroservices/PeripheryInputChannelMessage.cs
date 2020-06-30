namespace CloudMicroServices
{
    public class PeripheryInputChannelMessage : PeripheryChannelMessage
    {
        public ulong CorrelationId { get; set; }

        public override string ToString()
        {
            return $"ID {CorrelationId}";
        }
    }
}