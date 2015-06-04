using System;
using System.Reactive.Linq;
using Communications.Appi.Buffers;

namespace Communications.Appi.Ports
{
    public class AppiCanPortFactory
    {
        public AppiCanPort produceCanPort(IObservable<AppiLineStatus> LineStatusFlow)
        {
            return new AppiCanPort(LineStatusFlow.SelectMany(line => line.Frames),
                                   LineStatusFlow.Select(line => line.SendQueueSize));
        }
    }
}
