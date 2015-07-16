using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.Transactions
{
    public class IsoTpTransmitTransaction
    {
        private readonly IObservable<IsoTpFrame> _rx;
        private readonly int _subframeCapacity;
        private readonly IObserver<IsoTpFrame> _tx;

        public IsoTpTransmitTransaction(IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx, int SubframeCapacity)
        {
            _rx = Rx;
            _tx = Tx;
            _subframeCapacity = SubframeCapacity;
        }

        public void Send(IsoTpPacket Packet, TimeSpan Timeout)
        {
            if (Packet.Data.Length <= _subframeCapacity)
                SendShort(Packet);
            else
                SendLong(Packet, Timeout);
        }

        private void SendShort(IsoTpPacket Packet) { _tx.OnNext(new SingleFrame(Packet.Data)); }

        private void SendLong(IsoTpPacket Packet, TimeSpan Timeout)
        {
            int[] sent = { 0 };
            IBuffer<byte> dataFlow = Packet.Data.Share();
            IEnumerable<ConsecutiveFrame> cfFlow = dataFlow.Buffer(ConsecutiveFrame.GetPayload(_subframeCapacity))
                                                           .Select((d, i) => new ConsecutiveFrame(d.ToArray(), i & 0x0f))
                                                           .Share();

            IObservable<ConsecutiveFrame> sendEngine = _rx.Do(ValidateFrameType)
                                                          .Cast<FlowControlFrame>()
                                                          .Do(CheckForAbort)
                                                          .Timeout(Timeout)
                                                          .Where(fc => fc.Flag == FlowControlFlag.ClearToSend)
                                                          .SelectMany(fc => cfFlow.Take(fc.BlockSize)
                                                                                  .ToObservable()
                                                                                  .Do(cf => Thread.Sleep(fc.SeparationTime)) // TODO: Сделать задержку лучше
                                                                                  .Do(cf => sent[0] += cf.Data.Length));

            using (sendEngine.Subscribe(_tx))
            {
                var firstFrame = new FirstFrame(dataFlow.Take(FirstFrame.GetPayload(_subframeCapacity)).ToArray(), Packet.Data.Length);
                sent[0] += firstFrame.Data.Length;
                _tx.OnNext(firstFrame);
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
