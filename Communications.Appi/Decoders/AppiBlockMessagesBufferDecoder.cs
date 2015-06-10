using System.Collections.Generic;
using Communications.Appi.Buffers;
using Communications.Appi.Devices;

namespace Communications.Appi.Decoders
{
    public class AppiBlockMessagesBufferDecoder : MessagesBufferDecoder<AppiLine>
    {
        private readonly IDictionary<AppiLine, AppiLineStatusDecoder> _lineStatusDecoders;

        public AppiBlockMessagesBufferDecoder(int SequentialNumberOffset, IDictionary<AppiLine, AppiLineStatusDecoder> LineStatusDecoders)
            : base(SequentialNumberOffset)
        {
            _lineStatusDecoders = LineStatusDecoders;
        }

        public override Buffer DecodeBuffer(byte[] Buff) { return new AppiBlockMessagesBuffer(Buff[0], GetLineStatuses(Buff, _lineStatusDecoders)); }
    }
}
