using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.ReceiveStates
{
    /// <summary>Состояние готовности к приёму первой транзакции</summary>
    public class ReadyToReceiveState : IsoTpStateBase
    {
        private readonly IObserver<IsoTpPacket> _observer;
        private readonly IObserver<IsoTpFrame> _tx;
        private readonly IsoTpConnectionParameters _connectionParameters;

        public ReadyToReceiveState(IObserver<IsoTpPacket> Observer, IObserver<IsoTpFrame> Tx, IsoTpConnectionParameters ConnectionParameters)
        {
            _observer = Observer;
            _tx = Tx;
            _connectionParameters = ConnectionParameters;
        }

        public override IIsoTpState Operate(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.First:
                    var ff = (FirstFrame)Frame;
                    var longTransactionContext = new IsoTpReceiveTransactionContext(ff.PacketSize, _observer, _tx, _connectionParameters);
                    longTransactionContext.Write(ff.Data);

                    var flowControlFrame = new FlowControlFrame(FlowControlFlag.ClearToSend, (byte)_connectionParameters.BlockSize, _connectionParameters.SeparationTime);
                    longTransactionContext.Tx.OnNext(flowControlFrame);

                    return new ConsecutiveReceiveState(longTransactionContext);

                case IsoTpFrameType.Single:
                    var sf = (SingleFrame)Frame;
                    var shortTransaction = new IsoTpReceiveTransactionContext(sf.Data.Length, _observer, _tx, _connectionParameters);
                    shortTransaction.Write(sf.Data);
                    shortTransaction.Submit();
                    return new ReadyToReceiveState(_observer, _tx, _connectionParameters);
            }
            return this;
        }

        public override void Dispose() { }
    }
}
