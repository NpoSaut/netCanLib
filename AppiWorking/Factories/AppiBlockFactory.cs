using System.Collections.Generic;
using System.Linq;
using Communications.Appi.Decoders;
using Communications.Appi.Devices;
using Communications.Appi.Encoders;
using Communications.Usb;

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

        private static readonly AppiSendFramesBufferLayout _sendFramesBufferLayout = new AppiSendFramesBufferLayout(0, 1, 3, 10);

        public AppiDevice<AppiLine> OpenDevice(IUsbSlot Slot)
        {
            return new AppiBlock(Slot.OpenDevice(),
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
                                 new AppiSendFramesBufferEncoder<AppiLine>(new DictionaryInterfaceCodeProvider<AppiLine>(new Dictionary<AppiLine, byte>
                                                                                                                         {
                                                                                                                             { AppiLine.Can1, 0x02 },
                                                                                                                             { AppiLine.Can2, 0x03 }
                                                                                                                         }), _sendFramesBufferLayout));
        }
    }
}
