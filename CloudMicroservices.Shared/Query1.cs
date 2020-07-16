namespace CloudMicroservices.Shared
{
    public class Query1 : IQuery
    {
        public string Data { get; set; }

        public Query1(string data)
        {
            Data = data;
        }
    }
}