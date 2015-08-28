using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Can;
using Communications.Transactions;

namespace Communications.Appi.Ports
{
    public class AppiCanPort : ICanPort
    {
        private readonly IConnectableObservable<InstantaneousTransaction<CanFrame>> _rx;
        private readonly IDisposable _rxConnection;
        private readonly Subject<CanFrame> _tx;

        private readonly HashSet<int> filter = new HashSet<int>(new[] { 0x66a8, 0x66c8, 0x66e8 });

        public AppiCanPort(IObservable<CanFrame> Rx, CanPortOptions Options)
        {
            this.Options = Options;
            _tx = new Subject<CanFrame>();

            _rx = Rx.Merge(_tx.Select(f => f.GetLoopbackFrame()))
                    .Select(f => new InstantaneousTransaction<CanFrame>(f))
                    .Publish();

            _rxConnection = _rx.Connect();

            Rx.Where(f => filter.Contains(f.Descriptor)).Subscribe(f => Debug.Print("CAN:                                 <-- {0}", f));
            _tx.Where(f => filter.Contains(f.Descriptor)).Subscribe(f => Debug.Print("CAN:                                 --> {0}", f));
        }

        public IObservable<CanFrame> TxOutput
        {
            get { return _tx; }
        }

        public IObservable<ITransaction<CanFrame>> Rx
        {
            get { return _rx; }
        }

        public IObserver<CanFrame> Tx
        {
            get { return _tx; }
        }

        public ITransaction<CanFrame> BeginSend(CanFrame Frame) { throw new NotImplementedException(); }

        public CanPortOptions Options { get; private set; }

        public void Dispose() { _rxConnection.Dispose(); }
    }
}
