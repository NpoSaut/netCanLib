using System;

namespace Communications.Protocols.IsoTP
{
    public interface IIsoTpConnection {
        IObservable<IsoTpPacket> Rx { get; }
        IObserver<IsoTpPacket> Tx { get; }
    }
}