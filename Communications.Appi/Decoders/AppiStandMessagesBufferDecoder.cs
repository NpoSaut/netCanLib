using System.Collections.Generic;
using Communications.Appi.Buffers;
using Communications.Appi.Devices;

namespace Communications.Appi.Decoders
{
    public class AppiStandMessagesBufferDecoder : MessagesBufferDecoder<AppiStandLine>
    {
        private readonly int _canCommutationStatusOffset;
        private readonly IDictionary<AppiStandCanCommutationState, IDictionary<AppiStandLine, AppiLineStatusDecoder>> _lineStatusDecoders;

        public AppiStandMessagesBufferDecoder(int SequentialNumberOffset, int CanCommutationStatusOffset,
                                              IDictionary<AppiStandCanCommutationState, IDictionary<AppiStandLine, AppiLineStatusDecoder>> LineStatusDecoders)
            : base(SequentialNumberOffset)
        {
            _canCommutationStatusOffset = CanCommutationStatusOffset;
            _lineStatusDecoders = LineStatusDecoders;
        }

        public override Buffer DecodeBuffer(byte[] Buff)
        {
            AppiStandCanCommutationState commutationState = GetCanCommutationStatus(Buff);
            IDictionary<AppiStandLine, AppiLineStatusDecoder> commutationRelatedLineStatusDecoders = _lineStatusDecoders[commutationState];
            return new AppiStandMessagesBuffer(Buff[0], commutationState, GetLineStatuses(Buff, commutationRelatedLineStatusDecoders));
        }

        private AppiStandCanCommutationState GetCanCommutationStatus(byte[] Buff)
        {
            return Buff[_canCommutationStatusOffset] == 0 ? AppiStandCanCommutationState.CanTech : AppiStandCanCommutationState.CanBusA;
        }
    }
}
