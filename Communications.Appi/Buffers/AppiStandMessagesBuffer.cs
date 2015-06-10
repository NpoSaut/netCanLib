using System.Collections.Generic;
using Communications.Appi.Devices;

namespace Communications.Appi.Buffers
{
    public class AppiStandMessagesBuffer : MessagesBuffer<AppiStandLine>
    {
        public AppiStandMessagesBuffer(int SequentialNumber, AppiStandCanCommutationState CanCommutationState,
                                       IDictionary<AppiStandLine, AppiLineStatus> LineStatuses) : base(SequentialNumber, LineStatuses)
        {
            this.CanCommutationState = CanCommutationState;
        }

        public AppiStandCanCommutationState CanCommutationState { get; private set; }
    }
}