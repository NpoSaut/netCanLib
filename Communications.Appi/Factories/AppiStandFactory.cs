using System.Collections.Generic;
using System.Linq;
using Communications.Appi.Decoders;
using Communications.Appi.Devices;
using Communications.Appi.Encoders;
using Communications.Usb;

namespace Communications.Appi.Factories
{
    public class AppiStandFactory : AppiFactoryBase<AppiStandLine>
    {
        private const int SequentialNumberOffset = 5;

        private static readonly AppiSendFramesBufferLayout _sendFramesBufferLayout = new AppiSendFramesBufferLayout(0, 1, 3, 10);
        private static readonly AppiSetBaudRateBufferLayout _setBaudRateBufferLayout = new AppiSetBaudRateBufferLayout(0, 1, 2);

        private static readonly IDictionary<int, AppiLineStatusLayout> _buffersLayouts =
            new Dictionary<int, AppiLineStatusLayout>
            {
                { 1, new AppiLineStatusLayout(24, 6, 7, 424, 0, 0) },
                { 2, new AppiLineStatusLayout(224, 2, 9, 426, 0, 0) },
                { 3, new AppiLineStatusLayout(524, 3, 15, 425, 0, 0) },
                { 4, new AppiLineStatusLayout(724, 4, 17, 427, 0, 0) }
            };

        private static readonly IDictionary<AppiStandCanCommutationState, IDictionary<AppiStandLine, AppiLineStatusLayout>> _layouts =
            new Dictionary<AppiStandCanCommutationState, IDictionary<AppiStandLine, AppiLineStatusLayout>>
            {
                {
                    AppiStandCanCommutationState.CanBusA,
                    new Dictionary<AppiStandLine, AppiLineStatusLayout>
                    {
                        { AppiStandLine.CanA, _buffersLayouts[1] },
                        { AppiStandLine.CanB, _buffersLayouts[2] },
                        { AppiStandLine.CanBusA, _buffersLayouts[3] },
                        { AppiStandLine.CanBusB, _buffersLayouts[4] }
                    }
                },
                {
                    AppiStandCanCommutationState.CanTech,
                    new Dictionary<AppiStandLine, AppiLineStatusLayout>
                    {
                        { AppiStandLine.CanA, _buffersLayouts[1] },
                        { AppiStandLine.CanB, _buffersLayouts[2] },
                        { AppiStandLine.CanTech, _buffersLayouts[3] },
                        { AppiStandLine.CanBusB, _buffersLayouts[4] }
                    }
                }
            };

        public AppiStandFactory(IUsbFacade UsbFacade) : base("3af3f480-41b5-4c24-b2a9-6aacf7de3d01", UsbFacade) { }

        protected override AppiDevice<AppiStandLine> OpenDeviceImplementation(IAppiDeviceInfo DeviceInfo)
        {
            var interfaceCodeProvider = new DictionaryInterfaceCodeProvider<AppiStandLine>(new Dictionary<AppiStandLine, byte>
                                                                                           {
                                                                                               { AppiStandLine.CanA, 0x02 },
                                                                                               { AppiStandLine.CanB, 0x03 },
                                                                                               { AppiStandLine.CanBusA, 0x22 },
                                                                                               { AppiStandLine.CanBusB, 0x23 },
                                                                                               { AppiStandLine.CanTech, 0x12 },
                                                                                           });
            return new AppiStand(DeviceInfo.UsbSlot.OpenDevice(2048),
                                 new KeyBasedCompositeBufferDecoder(
                                     new Dictionary<byte, IAppiBufferDecoder>
                                     {
                                         {
                                             0x02,
                                             new AppiStandMessagesBufferDecoder(SequentialNumberOffset, 23,
                                                                                _layouts.ToDictionary(x => x.Key,
                                                                                                      x => GetLayouts(x.Value)))
                                         },
                                         { 0x09, new VersionBufferDecoder(SequentialNumberOffset, 6) }
                                     }),
                                 new AppiSendFramesBufferEncoder<AppiStandLine>(interfaceCodeProvider, _sendFramesBufferLayout),
                                 new AppiSetBaudRateBufferEncoder<AppiStandLine>(interfaceCodeProvider, _setBaudRateBufferLayout));
        }

        private static IDictionary<AppiStandLine, AppiLineStatusDecoder> GetLayouts(IDictionary<AppiStandLine, AppiLineStatusLayout> x)
        {
            return x.ToDictionary(y => y.Key, y => new AppiLineStatusDecoder(y.Value));
        }
    }
}
