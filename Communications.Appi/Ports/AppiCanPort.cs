using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Can;
using Communications.PortHelpers;
using Communications.Transactions;
using NLog;

namespace Communications.Appi.Ports
{
    public class AppiCanPort : ICanPort
    {
        private static readonly ILogger _logger = LogManager.GetLogger("CAN");
        private readonly IConnectableObservable<ITransaction<CanFrame>> _rx;
        private readonly IDisposable _rxConnection;
        private readonly Subject<AppiCanTransmitTransaction> _tx;

        private readonly HashSet<int> filter = new HashSet<int>(new[] { 0x66a8, 0x66c8, 0x66e8 });

        public AppiCanPort(IObservable<CanFrame> Rx, CanPortOptions Options)
        {
            this.Options = Options;
            _tx = new Subject<AppiCanTransmitTransaction>();

            _rx = Rx.Select(f => new InstantaneousTransaction<CanFrame>(f))
                    //.OfType<ITransaction<CanFrame>>()
                    //.Merge(_tx.SelectTransaction(t => t.GetLoopbackFrame()))
                    .Publish();

            _rxConnection = _rx.Connect();

            //Rx.Where(f => filter.Contains(f.Descriptor)).Subscribe(f => _logger.Debug("CAN:                                 <-- {0}", f));
            //_tx.Where(f => filter.Contains(f.Descriptor)).Subscribe(f => _logger.Debug("CAN:                                 --> {0}", f));
        }

        public Subject<AppiCanTransmitTransaction> TxOutput
        {
            get { return _tx; }
        }

        public IObservable<ITransaction<CanFrame>> Rx
        {
            get { return _rx; }
        }

        public IObserver<CanFrame> Tx
        {
            get { throw new NotImplementedException(); }
        }

        public ITransaction<CanFrame> BeginSend(CanFrame Frame)
        {
            var transaction = new AppiCanTransmitTransaction(Frame);
            _tx.OnNext(transaction);
            return transaction;
        }

        public CanPortOptions Options { get; private set; }

        public void Dispose() { _rxConnection.Dispose(); }
    }
}
