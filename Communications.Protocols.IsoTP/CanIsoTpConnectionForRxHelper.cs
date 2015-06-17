using System;
using System.Reactive;
using System.Reactive.Linq;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public static class CanIsoTpConnectionForRxHelper
    {
        public static IObservable<IsoTpPacket> IsoTpReceive(this IObservable<CanFrame> Rx, IObserver<CanFrame> Tx,
                                                            ushort TransmitDescriptor, ushort ReceiveDescriptor,
                                                            byte ReceiveBlockSize = 128, int SeparationTimeMs = 0)
        {
            return Rx.Where(f => f.Descriptor == ReceiveDescriptor)
                     .Select(f => IsoTpFrame.ParsePacket(f.Data))
                     .IsoTpReceive(Observer.Create<IsoTpFrame>(f => Tx.OnNext(f.GetCanFrame(TransmitDescriptor))),
                                   ReceiveBlockSize, SeparationTimeMs);
        }
    }

    public static class IsoTpConnectionForRxHelper
    {
        public static IObservable<IsoTpPacket> IsoTpReceive(this IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx,
                                                            byte ReceiveBlockSize = 128, int SeparationTimeMs = 0)
        {
            return new IsoReceiveObservable(Rx, Tx, ReceiveBlockSize, TimeSpan.FromMilliseconds(SeparationTimeMs));
        }
    }
}
