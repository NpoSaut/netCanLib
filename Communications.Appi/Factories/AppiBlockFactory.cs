using System.Collections.Generic;
using System.Linq;
using Communications.Appi.Decoders;
using Communications.Appi.Devices;
using Communications.Appi.Encoders;
using Communications.Usb;

namespace Communications.Appi.Factories
{
    public class AppiBlockFactory : AppiFactoryBase<AppiLine>
    {
        private const int SequentialNumberOffset = 5;

        private static readonly AppiSendFramesBufferLayout _sendFramesBufferLayout = new AppiSendFramesBufferLayout(0, 1, 3, 10);
        private static readonly AppiSetBaudRateBufferLayout _setBaudRateBufferLayout = new AppiSetBaudRateBufferLayout(0, 1, 2);

        private readonly IDictionary<AppiLine, AppiLineStatusLayout> _layouts =
            new Dictionary<AppiLine, AppiLineStatusLayout>
            {
                { AppiLine.Can1, new AppiLineStatusLayout(24, 6, 7, 17, 0, 0) },
                { AppiLine.Can2, new AppiLineStatusLayout(524, 2, 9, 19, 0, 0) }
            };

        public AppiBlockFactory(IUsbFacade UsbFacade) : base("524cc09a-0a72-4d06-980e-afee3131196e", UsbFacade) { }

        protected override AppiDevice<AppiLine> OpenDeviceImplementation(IAppiDeviceInfo DeviceInfo)
        {
            var interfaceCodeProvider = new DictionaryInterfaceCodeProvider<AppiLine>(new Dictionary<AppiLine, byte>
                                                                                      {
                                                                                          { AppiLine.Can1, 0x02 },
                                                                                          { AppiLine.Can2, 0x03 }
                                                                                      });
            return new AppiBlock(DeviceInfo.UsbSlot.OpenDevice(2048),
                                 new KeyBasedCompositeBufferDecoder(
                                     new Dictionary<byte, IAppiBufferDecoder>
                                     {
                                         {
                                             0x02,
                                             new AppiBlockMessagesBufferDecoder(SequentialNumberOffset,
                                                                                _layouts.ToDictionary(x => x.Key, x => new AppiLineStatusDecoder(x.Value)))
                                         },
                                         { 0x09, new VersionBufferDecoder(SequentialNumberOffset, 6) }
                                     }),
                                 new AppiSendFramesBufferEncoder<AppiLine>(interfaceCodeProvider, _sendFramesBufferLayout),
                                 new AppiSetBaudRateBufferEncoder<AppiLine>(interfaceCodeProvider, _setBaudRateBufferLayout));
        }
    }
}
