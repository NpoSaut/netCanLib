using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.ReceiveStates
{
    /// <summary>Состояние готовности к приёму первой транзакции</summary>
    public class ReadyToReceiveState : IsoTpStateBase
    {
        private readonly byte _blockSize;
        private readonly IObserver<IsoTpPacket> _observer;
        private readonly TimeSpan _separationTime;
        private readonly IObserver<IsoTpFrame> _tx;

        public ReadyToReceiveState(IObserver<IsoTpPacket> Observer, IObserver<IsoTpFrame> Tx, byte BlockSize, TimeSpan SeparationTime)
        {
            _observer = Observer;
            _tx = Tx;
            _blockSize = BlockSize;
            _separationTime = SeparationTime;
        }

        public override IIsoTpState Operate(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.First:
                    var ff = (FirstFrame)Frame;
                    var longTransactionContext = new IsoTpReceiveTransactionContext(ff.PacketSize, _observer, _tx, _blockSize, _separationTime);
                    longTransactionContext.Write(ff.Data);

                    var flowControlFrame = new FlowControlFrame(FlowControlFlag.ClearToSend, _blockSize, _separationTime);
                    longTransactionContext.Tx.OnNext(flowControlFrame);

                    return new ConsecutiveReceiveState(longTransactionContext);

                case IsoTpFrameType.Single:
                    var sf = (SingleFrame)Frame;
                    var shortTransaction = new IsoTpReceiveTransactionContext(sf.Data.Length, _observer, _tx, _blockSize, _separationTime);
                    shortTransaction.Write(sf.Data);
                    shortTransaction.Submit();
                    return new ReadyToReceiveState(_observer, _tx, _blockSize, _separationTime);
            }
            return this;
        }
    }
}
