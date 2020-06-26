namespace CloudMicroServices
{
    public class PeripheryChannelMessage
    {
        public ulong CorrelationId { get; set; }
        public IQuery Query { get; set; }

        public override string ToString()
        {
            return $"ID {CorrelationId} Data {Query.Data}";
        }
    }
}