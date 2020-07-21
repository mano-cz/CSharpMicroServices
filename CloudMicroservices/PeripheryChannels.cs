using System.Collections.Concurrent;
using System.Threading.Channels;
using CloudMicroservices.Shared;

namespace CloudMicroServices
{
    public class PeripheryChannels// : IProducerConsumerCollection<byte[]>
    {
        public Channel<InputChannelMessage> Input { get; } = Channel.CreateUnbounded<InputChannelMessage>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        public Channel<uint> InputMetadataConfirmation { get; } = Channel.CreateBounded<uint>(1);

        public ConcurrentDictionary<uint, Channel<ChannelMessage>> Output { get; } = new ConcurrentDictionary<uint, Channel<ChannelMessage>>();

        public static Channel<ChannelMessage> CreateOutputChannel()
        {
            return Channel.CreateUnbounded<ChannelMessage>(
                new UnboundedChannelOptions
                {
                    SingleWriter = true,
                    SingleReader = true
                });
        }
    }
}