namespace CloudMicroservices.Shared
{
    public interface IMessageProcessor
    {
        byte[] ProcessData(byte[] data);
        void ProcessMetadata(byte[] metadata);
        object Deserialize(byte[] data);
        void Initialize(IMessageProcessor other);
    }
}