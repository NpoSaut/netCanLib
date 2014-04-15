using System;
using System.Linq;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class CanIsoTpConnection : IsoTpConnectionBase
    {
        public CanIsoTpConnection(CanFlow Flow, ushort TransmitDescriptor, ushort ReceiveDescriptor, int ReceiveBlockSize = 128,
                                  int SeparationTimeMs = 0) : base(ReceiveBlockSize, SeparationTimeMs)
        {
            this.Flow = Flow;
            this.ReceiveDescriptor = ReceiveDescriptor;
            this.TransmitDescriptor = TransmitDescriptor;
        }

        private CanFlow Flow { get; set; }
        private ushort TransmitDescriptor { get; set; }
        private ushort ReceiveDescriptor { get; set; }

        public override int SubframeLength
        {
            get { return 8; }
        }

        public override IsoTpFrame ReadNextFrame(TimeSpan Timeout)
        {
            IsoTpFrame frame = Flow.Read(Timeout)
                                   .Where(f => f.Descriptor == ReceiveDescriptor)
                                   .Select(f => IsoTpFrame.ParsePacket(f.Data))
                                   .First();
            return frame;
        }

        public override void SendFrame(IsoTpFrame Frame) { Flow.Send(Frame.GetCanFrame(TransmitDescriptor)); }
    }
}