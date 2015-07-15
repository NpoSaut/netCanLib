using System;
using System.Reactive.Linq;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class IsoTpConnection : IIsoTpConnection
    {
        private readonly IsoReceiveObservable _receiver;
        private readonly IsoTpSendObserver _sender;

        public IsoTpConnection(IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx, byte ReceiveBlockSize, TimeSpan SeparationTime, TimeSpan Timeout,
                               int FrameLayerCapacity)
        {
            IObservable<IsoTpFrame> sharedRx = Rx.Publish().RefCount();
            _receiver = new IsoReceiveObservable(sharedRx, Tx, ReceiveBlockSize, SeparationTime);
            _sender = new IsoTpSendObserver(sharedRx, Tx, Timeout, FrameLayerCapacity);
        }

        public IObservable<IsoTpPacket> Rx
        {
            get { return _receiver; }
        }

        public IObserver<IsoTpPacket> Tx
        {
            get { return _sender; }
        }
    }
}
