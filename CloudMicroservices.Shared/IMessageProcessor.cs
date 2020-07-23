namespace CloudMicroservices.Shared
{
    public interface IMessageProcessor
    {
        byte[] ProcessData(byte[] data);
        void ProcessMetadata(byte[] metadata);
        void Initialize(IMessageProcessor other);
    }
}