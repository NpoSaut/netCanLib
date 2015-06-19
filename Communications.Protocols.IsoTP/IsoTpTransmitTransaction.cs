using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class IsoTpTransmitTransaction
    {
        public IDisposable Begin(IsoTpPacket Packet, IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx, TimeSpan Timeout)
        {
            IBuffer<byte> dataFlow = Packet.Data.Share();
            IEnumerable<ConsecutiveFrame> cfFlow = dataFlow.Buffer(ConsecutiveFrame.GetPayload(8))
                                                           .Select((d, i) => new ConsecutiveFrame(d.ToArray(), i & 0x0f));

            IDisposable disposable = Rx.Do(Validate)
                                       .Cast<FlowControlFrame>()
                                       .Do(CheckForAbort)
                                       .Timeout(Timeout)
                                       .Where(fc => fc.Flag == FlowControlFlag.ClearToSend)
                                       .SelectMany(fc => cfFlow.Take(fc.BlockSize)
                                                               .ToObservable()
                                                               .Delay(fc.SeparationTime))
                                       .Subscribe(Tx);

            Tx.OnNext(new FirstFrame(dataFlow.Take(FirstFrame.GetPayload(8)).ToArray(), Packet.Data.Length));

            return disposable;
        }

        private void CheckForAbort(FlowControlFrame Frame)
        {
            if (Frame.Flag == FlowControlFlag.Abort)
                throw new IsoTpTransactionAbortedException();
        }

        private void Validate(IsoTpFrame Frame)
        {
            if (!(Frame is FlowControlFrame))
                throw new IsoTpWrongFrameException(Frame, typeof (FlowControlFrame));
        }
    }
}
