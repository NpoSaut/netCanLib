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

        public static IIsoTpConnection OpenIsoTpConnection(this ICanPort Port, ushort TransmitDescriptor, ushort ReceiveDescriptor, byte ReceiveBlockSize,
                                                           TimeSpan SeparationTime, TimeSpan Timeout)
        {
            return new IsoTpConnection(
                Port.Rx
                    .Where(f => f.Descriptor == ReceiveDescriptor)
                    .Select(f => IsoTpFrame.ParsePacket(f.Data)),
                Observer.Create<IsoTpFrame>(f => Port.Tx.OnNext(f.GetCanFrame(TransmitDescriptor))),
                ReceiveBlockSize, SeparationTime, Timeout, 8);
        }

        #region Overloads

        public static IIsoTpConnection OpenIsoTpConnection(this ICanPort Port, ushort TransmitDescriptor, ushort ReceiveDescriptor, TimeSpan Timeout)
        {
            return OpenIsoTpConnection(Port, TransmitDescriptor, ReceiveDescriptor, 128, Timeout);
        }

        public static IIsoTpConnection OpenIsoTpConnection(this ICanPort Port, ushort TransmitDescriptor, ushort ReceiveDescriptor, byte ReceiveBlockSize,
                                                           TimeSpan Timeout)
        {
            return OpenIsoTpConnection(Port, TransmitDescriptor, ReceiveDescriptor, ReceiveBlockSize, TimeSpan.Zero, Timeout);
        }

        public static IIsoTpConnection OpenIsoTpConnection(this ICanPort Port, ushort TransmitDescriptor, byte ReceiveBlockSize,
                                                           TimeSpan SeparationTime, TimeSpan Timeout)
        {
            return OpenIsoTpConnection(Port, TransmitDescriptor, 128, ReceiveBlockSize, SeparationTime, Timeout);
        }

        #endregion
    }
}
