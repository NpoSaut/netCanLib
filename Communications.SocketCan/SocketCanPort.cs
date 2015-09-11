using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Communications.Can;
using Communications.PortHelpers;
using Communications.SocketCan.LinuxSockets;
using Communications.Transactions;

namespace Communications.SocketCan
{
    public class SocketCanPort : ICanPort
    {
        private readonly IDisposable _rxConnection;
        private readonly EventLoopScheduler _scheduler;

        private readonly ILinuxSocket _socketIn;
        private readonly ILinuxSocket _socketOut;

        private readonly IDictionary<CanFrame, SocketCanBlockedTransmitTransaction> _transactions;

        public SocketCanPort(string Name, ILinuxSocket SocketIn, ILinuxSocket SocketOut)
        {
            _socketIn = SocketIn;
            _socketOut = SocketOut;
            _scheduler = new EventLoopScheduler(ts => new Thread(ts) { Name = string.Format("{0} socket thread", Name) });

            _transactions = new Dictionary<CanFrame, SocketCanBlockedTransmitTransaction>(new CanFrameEqualityComparer());

            IConnectableObservable<ITransaction<CanFrame>> rx =
                Observable.Interval(TimeSpan.Zero, _scheduler)
                          .Select(i => (ITransaction<CanFrame>)new InstantaneousTransaction<CanFrame>(_socketIn.Receive(TimeSpan.MaxValue)))
                          .Publish();
            Rx = rx;

            rx.WaitForTransactionCompleated()
              .Subscribe(CommitTransaction);

            _rxConnection = rx.Connect();
        }

        /// <summary>Выполняет определяемые приложением задачи, связанные с высвобождением или сбросом неуправляемых ресурсов.</summary>
        public void Dispose()
        {
            _rxConnection.Dispose();
            _scheduler.Dispose();
        }

        /// <summary>Поток входящих сообщений</summary>
        public IObservable<ITransaction<CanFrame>> Rx { get; private set; }

        /// <summary>Поток исходящих сообщений</summary>
        public IObserver<CanFrame> Tx { get; private set; }

        /// <summary>Опции порта</summary>
        public CanPortOptions Options { get; private set; }

        /// <summary>Начинает отправку кадра</summary>
        /// <param name="Frame">Кадр для отправки</param>
        /// <returns>Транзакция передачи</returns>
        public ITransaction<CanFrame> BeginSend(CanFrame Frame)
        {
            var transaction = new SocketCanBlockedTransmitTransaction(Frame);
            _transactions.Add(Frame, transaction);

            return transaction;
        }

        private void CommitTransaction(CanFrame Frame)
        {
            SocketCanBlockedTransmitTransaction transaction;
            if (_transactions.TryGetValue(Frame, out transaction))
                transaction.Commit();
        }
    }

    internal class SocketCanBlockedTransmitTransaction : LongTransactionBase<CanFrame>
    {
        private readonly CanFrame _payload;
        public SocketCanBlockedTransmitTransaction(CanFrame Payload) { _payload = Payload; }

        protected override CanFrame GetPayload() { return _payload; }
    }
}
