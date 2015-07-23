using Communications.Can;

namespace Communications.Protocols.IsoTP
{
    public static class CanIsoTpConnectionForRxHelper
    {
        public static IIsoTpConnection OpenIsoTpConnection(this ICanPort Port, ushort TransmitDescriptor, ushort ReceiveDescriptor,
                                                           IsoTpConnectionParameters ConnectionParameters)
        {
            return new IsoTpOverCanPort(Port, TransmitDescriptor, ReceiveDescriptor, ConnectionParameters);
        }
    }
}
