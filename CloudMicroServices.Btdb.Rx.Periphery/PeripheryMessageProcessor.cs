using System;
using System.Threading;
using CloudMicroservices.Shared;

namespace CloudMicroServices.Btdb.Rx.Periphery
{
    public class PeripheryMessageProcessor
    {
        readonly ChannelDataSerializer _serializer;

        public PeripheryMessageProcessor(ChannelDataSerializer serializer)
        {
            _serializer = serializer;
        }

        // public ChannelMessage Process(ChannelMessage message)
        // {
        //     var nextQuery = (Query1)_serializer.Deserialize(message.MetaData, message.Data);
        //     // Console.WriteLine($"Received next message: {nextQuery.Data}");
        //     var response = new Response1 { Data = $"{nextQuery.Data}Response" };
        //     var (meta, data) = _serializer.Serialize(response);
        //     // Thread.Sleep(100);
        //     return new ChannelMessage
        //     {
        //         Data = data,
        //         MetaData = meta
        //     };
        // }
    }
}