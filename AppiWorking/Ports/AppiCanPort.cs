using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Can;

namespace Communications.Appi.Ports
{
    public class AppiCanPort : ICanPort
    {
        private readonly IObservable<int> _sendQueueSize;
        private readonly Subject<CanFrame> _tx;

        public AppiCanPort(IObservable<CanFrame> Rx, IObservable<int> SendQueueSize)
        {
            _sendQueueSize = SendQueueSize;
            this.Rx = Rx;

            _tx = new Subject<CanFrame>();
        }

        public IObservable<CanFrame> Rx { get; private set; }

        public void Send(CanFrame Frame) { _tx.OnNext(Frame); }

        public void SubscribeSend(IObservable<CanFrame> FramesFlow) { FramesFlow.Subscribe(_tx); }
    }
}
