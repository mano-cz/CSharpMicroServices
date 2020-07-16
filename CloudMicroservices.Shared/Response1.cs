namespace CloudMicroservices.Shared
{
    public class Response1 : IResponse
    {
        public string Data { get; set; }

        public override string ToString()
        {
            return Data;
        }
    }
}