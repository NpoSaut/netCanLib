using System;
using Communications.Protocols.IsoTP.Contexts;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States.Receive
{
    /// <summary>Состояние отправки последовательных фреймов</summary>
    public class ConsecutiveReceiveState : IsoTpStateBase
    {
        private readonly IsoTpReceiveTransactionContext _transactionContext;
        private int _counter;

        public ConsecutiveReceiveState(IsoTpReceiveTransactionContext TransactionContext)
        {
            _transactionContext = TransactionContext;
            _counter = 0;
        }

        public override IIsoTpState Operate(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.Consecutive:
                    var cf = (ConsecutiveFrame)Frame;

                    if (cf.Index != _transactionContext.ExpectedFrameIndex)
                        _transactionContext.OnError(new IsoTpSequenceException(_transactionContext.ExpectedFrameIndex, cf.Index));

                    _transactionContext.IncreaseFrameIndex();
                    _transactionContext.Write(cf.Data);
                    _counter++;

                    if (_counter == _transactionContext.BlockSize)
                    {
                        var flowControlFrame = new FlowControlFrame(FlowControlFlag.ClearToSend, _transactionContext.BlockSize,
                                                                    _transactionContext.SeparationTime);
                        _transactionContext.Tx.OnNext(flowControlFrame);

                        return new ConsecutiveReceiveState(_transactionContext);
                    }
                    if (_transactionContext.IsDone)
                    {
                        _transactionContext.Submit();
                        return new ReadyToReceiveState(_transactionContext.Observer, _transactionContext.Tx, _transactionContext.BlockSize,
                                                       _transactionContext.SeparationTime);
                    }
                    break;

                case IsoTpFrameType.First:
                case IsoTpFrameType.FlowControl:
                case IsoTpFrameType.Single:
                    _transactionContext.OnError(new IsoTpWrongFrameException(Frame, typeof (ConsecutiveFrame));
                    break;
            }
            return this;
        }

        public override void OnException(Exception e)
        {
            _transactionContext.Tx.OnNext(FlowControlFrame.AbortFrame);
            base.OnException(e);
        }
    }
}
