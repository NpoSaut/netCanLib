using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class IsoTpTransmitTransaction
    {
        public void Begin(IsoTpPacket Packet, IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx, TimeSpan Timeout)
        {
            int[] sent = { 0 };
            IBuffer<byte> dataFlow = Packet.Data.Share();
            IEnumerable<ConsecutiveFrame> cfFlow = dataFlow.Buffer(ConsecutiveFrame.GetPayload(8))
                                                           .Select((d, i) => new ConsecutiveFrame(d.ToArray(), i & 0x0f));

            IObservable<ConsecutiveFrame> sendEngine = Rx.Do(ValidateFrameType)
                                                         .Cast<FlowControlFrame>()
                                                         .Do(CheckForAbort)
                                                         .Timeout(Timeout)
                                                         .Where(fc => fc.Flag == FlowControlFlag.ClearToSend)
                                                         .SelectMany(fc => cfFlow.Take(fc.BlockSize)
                                                                                 .ToObservable()
                                                                                 .Delay(fc.SeparationTime)
                                                                                 .Do(cf => sent[0] += cf.Data.Length));

            using (sendEngine.Subscribe(Tx))
            {
                var firstFrame = new FirstFrame(dataFlow.Take(FirstFrame.GetPayload(8)).ToArray(), Packet.Data.Length);
                sent[0] += firstFrame.Data.Length;
                Tx.OnNext(firstFrame);
                SpinWait.SpinUntil(() => sent[0] == Packet.Data.Length);
            }
        }

        private void ValidateFrameType(IsoTpFrame Frame)
        {
            if (!(Frame is FlowControlFrame))
                throw new IsoTpWrongFrameException(Frame, typeof (FlowControlFrame));
        }

        private void CheckForAbort(FlowControlFrame Frame)
        {
            if (Frame.Flag == FlowControlFlag.Abort)
                throw new IsoTpTransactionAbortedException();
        }
    }
}
