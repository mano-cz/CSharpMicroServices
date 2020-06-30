using System.Collections.Concurrent;
using System.Threading.Channels;

namespace CloudMicroServices
{
    public class PeripheryChannels// : IProducerConsumerCollection<byte[]>
    {
        public Channel<PeripheryInputChannelMessage> Input { get; } = Channel.CreateUnbounded<PeripheryInputChannelMessage>();
        public ConcurrentDictionary<ulong, Channel<PeripheryChannelMessage>> Output { get; } = new ConcurrentDictionary<ulong, Channel<PeripheryChannelMessage>>();
        public ChannelDataSerializer Serializer { get; } = new ChannelDataSerializer();
    }
}