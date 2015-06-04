using System.Collections.Generic;
using System.Linq;
using Communications.Appi.Decoders;
using Communications.Appi.Devices;
using Communications.Usb;

namespace Communications.Appi.Factories
{
    public class AppiStandFactory : IAppiFactory<AppiStandLine>
    {
        private const int SequentialNumberOffset = 5;

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

        public AppiDevice<AppiStandLine> OpenDevice(IUsbSlot Slot)
        {
            return new AppiStand(Slot.OpenDevice(),
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
                                     }));
        }

        private static IDictionary<AppiStandLine, AppiLineStatusDecoder> GetLayouts(IDictionary<AppiStandLine, AppiLineStatusLayout> x)
        {
            return x.ToDictionary(y => y.Key, y => new AppiLineStatusDecoder(y.Value));
        }
    }
}