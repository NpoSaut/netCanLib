using Communications.Protocols.IsoTP.Transactions;

namespace Communications.Protocols.IsoTP
{
    public class IsoTpOverCanPortOptions : DataPortOptions<IsoTpPacket>
    {
        public IsoTpOverCanPortOptions(ushort TransmitDescriptor, ushort ReceiveDescriptor) : base(4095)
        {
            this.TransmitDescriptor = TransmitDescriptor;
            this.ReceiveDescriptor = ReceiveDescriptor;
        }

        public ushort TransmitDescriptor { get; set; }
        public ushort ReceiveDescriptor { get; set; }
    }
}