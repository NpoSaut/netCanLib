using System;
using System.Reactive.Subjects;
using Communications.Can;

namespace Communications.Appi.Ports
{
    public class AppiCanPort : ICanPort
    {
        private readonly Subject<CanFrame> _tx;

        public AppiCanPort(IObservable<CanFrame> Rx)
        {
            this.Rx = Rx;
            _tx = new Subject<CanFrame>();
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
    }
}
