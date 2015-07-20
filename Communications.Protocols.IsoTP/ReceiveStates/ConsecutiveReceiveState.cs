using System;
using System.Diagnostics;
using System.Timers;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.ReceiveStates
{
    /// <summary>Состояние отправки последовательных фреймов</summary>
    public class ConsecutiveReceiveState : IsoTpStateBase
    {
        private readonly Timer _timer;
        private readonly IsoTpReceiveTransactionContext _transactionContext;
        private int _counter;
        private Stopwatch _sw;
        private TimeSpan _ts = TimeSpan.Zero;

        public ConsecutiveReceiveState(IsoTpReceiveTransactionContext TransactionContext)
        {
            _transactionContext = TransactionContext;
            _counter = 0;
            _timer = new Timer(_transactionContext.Timeout.TotalMilliseconds) { AutoReset = true };
            _timer.Elapsed += OnTimeout;
            _timer.Start();

            _sw = new Stopwatch();
            _sw.Start();
        }

        private void OnTimeout(object Sender, ElapsedEventArgs Args)
        {
            var xxx = _sw.Elapsed;
            _transactionContext.Observer.OnError(new IsoTpTimeoutException());
        }

        public override IIsoTpState Operate(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.Consecutive:
                    var cf = (ConsecutiveFrame)Frame;

                    Debug.Print("{0}: CF Successfully received ({1})", _sw.Elapsed, (_sw.Elapsed - _ts));
                    _ts = _sw.Elapsed;

                    if (cf.Index != _transactionContext.ExpectedFrameIndex)
                        _transactionContext.OnError(new IsoTpSequenceException(_transactionContext.ExpectedFrameIndex, cf.Index));

                    _transactionContext.IncreaseFrameIndex();
                    _transactionContext.Write(cf.Data);
                    _timer.Stop(); _timer.Start();
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
                                                       _transactionContext.SeparationTime, _transactionContext.Timeout);
                    }
                    break;

                case IsoTpFrameType.First:
                case IsoTpFrameType.FlowControl:
                case IsoTpFrameType.Single:
                    _transactionContext.OnError(new IsoTpWrongFrameException(Frame, typeof (ConsecutiveFrame)));
                    break;
            }
            return this;
        }

        public override IIsoTpState OnException(Exception e)
        {
            _transactionContext.Tx.OnNext(FlowControlFrame.AbortFrame);
            base.OnException(e);
            return new ReadyToReceiveState(_transactionContext.Observer, _transactionContext.Tx, _transactionContext.BlockSize,
                                           _transactionContext.SeparationTime, _transactionContext.Timeout);
        }

        public override void Dispose() { _timer.Dispose(); }
    }
}
