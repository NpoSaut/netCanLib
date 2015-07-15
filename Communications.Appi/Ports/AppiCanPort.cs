using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Can;

namespace Communications.Appi.Ports
{
    public class AppiCanPort : ICanPort
    {
        private readonly Subject<CanFrame> _tx;

        public AppiCanPort(IObservable<CanFrame> Rx, CanPortOptions Options)
        {
            this.Options = Options;
            _tx = new Subject<CanFrame>();
            this.Rx = Rx.Merge(_tx.Select(f => f.GetLoopbackFrame())).Publish().RefCount();
        }

        public IObservable<CanFrame> TxOutput
        {
            get { return _tx; }
        }

        public IObservable<CanFrame> Rx { get; private set; }

        public IObserver<CanFrame> Tx
        {
            get { return _tx; }
        }

        public CanPortOptions Options { get; private set; }

        public void Dispose() { }
    }
}
