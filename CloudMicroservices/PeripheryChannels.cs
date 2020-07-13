using System.Collections.Concurrent;
using System.Threading.Channels;

namespace CloudMicroServices
{
    public class PeripheryChannels// : IProducerConsumerCollection<byte[]>
    {
        public Channel<PeripheryInputChannelMessage> Input { get; } = Channel.CreateUnbounded<PeripheryInputChannelMessage>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        public ConcurrentDictionary<uint, Channel<PeripheryChannelMessage>> Output { get; } = new ConcurrentDictionary<uint, Channel<PeripheryChannelMessage>>();

        public static Channel<PeripheryChannelMessage> CreateBoundedOutputChannel()
        {
            return Channel.CreateBounded<PeripheryChannelMessage>(
                new BoundedChannelOptions(1)
                {
                    SingleWriter = true,
                    SingleReader = true
                });
        }
    }
}