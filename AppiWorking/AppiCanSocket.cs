using System.Collections.Generic;
using System.Linq;
using Communications.Can;
using Communications.Sockets;

namespace Communications.Appi
{
    public class AppiCanSocket : BufferedSockedBase<CanFrame>, ICanSocket
    {
        public AppiLine Line { get; private set; }
        public AppiDev Appi { get; private set; }

        public AppiCanSocket(AppiDev Appi, AppiLine Line) : base(string.Format("Appi{0}", Line))
        {
            this.Line = Line;
            this.Appi = Appi;
        }

        public override void Send(IEnumerable<CanFrame> Frames)
        {
            Appi.SendFrames(Frames.ToList(), Line);
        }

        private bool _disposed = false;
        public override void Dispose()
        {
            if (_disposed) return;
            _disposed = false;

            Appi.OnSocketDisposed(this);
            base.Dispose();
        }
    }
}