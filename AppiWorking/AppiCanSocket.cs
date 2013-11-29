using System.Collections.Generic;
using System.Linq;
using Communications.Can;
using Communications.Can.FrameEncoders;
using Communications.Sockets;

namespace Communications.Appi
{
    public class AppiCanSocket : BufferedSocketBase<CanFrame>, ICanSocket
    {
        public AppiCanPort Port { get; private set; }

        public AppiCanSocket(string Name, AppiCanPort Port) : base(Name) { this.Port = Port; }

        public override void Send(IEnumerable<CanFrame> Frames)
        {
            Port.Send(Frames.ToList());
        }

        private bool _disposed = false;
        public override void Dispose()
        {
            if (_disposed) return;
            _disposed = false;

            base.Dispose();
        }
    }
}