using System.Collections.Generic;
using System.Linq;
using Communications.Appi.Decoders;
using Communications.Appi.Devices;

namespace Communications.Appi.Factories
{
    public class AppiBlockFactory : IAppiFactory<AppiLine>
    {
        private const int SequentialNumberOffset = 5;

        private static readonly IDictionary<AppiLine, AppiLineStatusLayout> _layouts =
            new Dictionary<AppiLine, AppiLineStatusLayout>
            {
                { AppiLine.Can1, new AppiLineStatusLayout(24, 6, 7, 17, 0, 0) },
                { AppiLine.Can2, new AppiLineStatusLayout(524, 2, 9, 19, 0, 0) }
            };

        public AppiDevice<AppiLine> OpenDevice(IUsbSlot Slot)
        {
            return new AppiBlock(Slot.OpenDevice(),
                                 new KeyBasedCompositeBufferDecoder(
                                     new Dictionary<byte, IAppiBufferDecoder>
                                     {
                                         {
                                             0x02,
                                             new AppiBlockMessagesBufferDecoder(SequentialNumberOffset, _layouts.ToDictionary(x => x.Key, x => new AppiLineStatusDecoder(x.Value)))
                                         },
                                         { 0x09, new VersionBufferDecoder(SequentialNumberOffset, 6) }
                                     }));
        }
    }
}
