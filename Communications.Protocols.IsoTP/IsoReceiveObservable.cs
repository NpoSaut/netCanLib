using System;
using System.Reactive;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;
using Communications.Protocols.IsoTP.States.Receive;

namespace Communications.Protocols.IsoTP
{
    internal class IsoReceiveObservable : ObservableBase<IsoTpPacket>
    {
        private readonly byte _receiveBlockSize;
        private readonly IObservable<IsoTpFrame> _rx;
        private readonly TimeSpan _separationTime;
        private readonly IObserver<IsoTpFrame> _tx;

        public IsoReceiveObservable(IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx,
                                    byte ReceiveBlockSize, TimeSpan SeparationTime)
        {
            _rx = Rx;
            _tx = Tx;
            _receiveBlockSize = ReceiveBlockSize;
            _separationTime = SeparationTime;
        }

        /// <summary>Implement this method with the core subscription logic for the observable sequence.</summary>
        /// <param name="observer">Observer to send notifications to.</param>
        /// <returns>Disposable object representing an observer's subscription to the observable sequence.</returns>
        protected override IDisposable SubscribeCore(IObserver<IsoTpPacket> observer)
        {
            var connection = new IsoTpReceiveTransaction(observer, _rx, _tx, _receiveBlockSize, _separationTime);
            return connection;
        }
    }

    internal class IsoTpReceiveTransaction : IDisposable
    {
        private readonly IDisposable _rxConnection;

        private IIsoTpState _currentState;

        public IsoTpReceiveTransaction(IObserver<IsoTpPacket> Observer,
                                       IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx,
                                       byte ReceiveBlockSize, TimeSpan SeparationTime)
        {
            _currentState = new ReadyToReceiveState(Observer, Tx, ReceiveBlockSize, SeparationTime);

            _rxConnection = Rx.Subscribe(f => _currentState = _currentState.Operate(f),
                                         e => _currentState.OnException(e));
        }

        /// <summary>
        ///     ¬ыполн€ет определ€емые приложением задачи, св€занные с удалением, высвобождением или сбросом неуправл€емых
        ///     ресурсов.
        /// </summary>
        public void Dispose() { _rxConnection.Dispose(); }
    }
}
