using System.Collections.Concurrent;
using System.Threading.Channels;

namespace CloudMicroServices
{
    public class PeripheryChannels// : IProducerConsumerCollection<byte[]>
    {
        // later byte[]
        public Channel<PeripheryChannelMessage> Input { get; } = Channel.CreateUnbounded<PeripheryChannelMessage>();
        public ConcurrentDictionary<ulong, Channel<IResponse>> Output { get; } = new ConcurrentDictionary<ulong, Channel<IResponse>>();
    }
}