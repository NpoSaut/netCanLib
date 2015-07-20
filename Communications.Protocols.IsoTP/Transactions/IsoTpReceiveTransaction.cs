using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.ReceiveStates;

namespace Communications.Protocols.IsoTP.Transactions
{
    internal class IsoTpReceiveTransaction : IDisposable
    {
        private readonly IDisposable _rxConnection;

        private readonly EventLoopScheduler _scheduler = new EventLoopScheduler();
        private IIsoTpState _currentState;

        public IsoTpReceiveTransaction(IObserver<IsoTpPacket> Observer, IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx, byte ReceiveBlockSize,
                                       TimeSpan SeparationTime, TimeSpan Timeout)
        {
            _currentState = new ReadyToReceiveState(Observer, Tx, ReceiveBlockSize, SeparationTime, Timeout);

            _rxConnection = Rx.ObserveOn(_scheduler)
                              .Subscribe(f => SetState(_currentState.Operate(f)),
                                         e => SetState(_currentState.OnException(e)));
        }

        /// <summary>
        ///     ¬ыполн€ет определ€емые приложением задачи, св€занные с удалением, высвобождением или сбросом неуправл€емых
        ///     ресурсов.
        /// </summary>
        public void Dispose()
        {
            _rxConnection.Dispose();
            _scheduler.Dispose();
        }

        private void SetState(IIsoTpState NewState)
        {
            if (_currentState != NewState)
            {
                _currentState.Dispose();
                _currentState = NewState;
            }
        }
    }
}
