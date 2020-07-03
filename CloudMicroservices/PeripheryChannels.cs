using System.Collections.Concurrent;
using System.Threading.Channels;

namespace CloudMicroServices
{
    public class PeripheryChannels// : IProducerConsumerCollection<byte[]>
    {
        public Channel<PeripheryInputChannelMessage> Input { get; } = Channel.CreateUnbounded<PeripheryInputChannelMessage>();
        public ConcurrentDictionary<uint, Channel<PeripheryChannelMessage>> Output { get; } = new ConcurrentDictionary<uint, Channel<PeripheryChannelMessage>>();
    }
}