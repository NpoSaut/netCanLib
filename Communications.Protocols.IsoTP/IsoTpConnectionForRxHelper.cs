using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public static class IsoTpConnectionForRxHelper
    {
        public static IObservable<IsoTpPacket> IsoTpReceive(this IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx, TimeSpan Timeout,
                                                            byte ReceiveBlockSize = 128, int SeparationTimeMs = 0)
        {
            return new IsoReceiveObservable(Rx, Tx, ReceiveBlockSize, TimeSpan.FromMilliseconds(SeparationTimeMs), Timeout);
        }
    }
}
