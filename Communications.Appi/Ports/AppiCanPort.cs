using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Communications.Can;

namespace Communications.Appi.Ports
{
    public class AppiCanPort : ICanPort
    {
        private readonly Subject<CanFrame> _tx;

        private HashSet<int> filter = new HashSet<int>(new[] { 0x66a8, 0x66c8, 0x66e8 });
                                                                       
        public AppiCanPort(IObservable<CanFrame> Rx, CanPortOptions Options)
        {
            this.Options = Options;
            _tx = new Subject<CanFrame>();
            this.Rx = Rx.Merge(_tx.Select(f => f.GetLoopbackFrame())).Publish().RefCount();

            Rx.Where(f => filter.Contains(f.Descriptor)).Subscribe(f => Debug.Print("CAN:                                 <-- {0}", f));
            _tx.Where(f => filter.Contains(f.Descriptor)).Subscribe(f => Debug.Print("CAN:                                 --> {0}", f));
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
