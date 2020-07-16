using BTDB.Service;
using CloudMicroservices.Shared;

namespace CloudMicroServices.Btdb.Rx.Periphery
{
    public class Periphery
    {
        readonly Service _inputService;

        public Periphery(IChannel inputChannel)
        {
            _inputService = new Service(inputChannel);
            _inputService.RegisterLocalService(new PeripheryMessageProcessor(new ChannelDataSerializer()));
        }
    }
}
