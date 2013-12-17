using System.Collections.Generic;
using System.Linq;
using Communications.Can;
using Communications.Can.FrameEncoders;
using Communications.Sockets;

namespace Communications.Appi
{
    public class AppiCanSocket : CanSocketBase
    {
        private ISendPipe<CanFrame> SendPipe { get; set; }

        public AppiCanSocket(string Name, ISendPipe<CanFrame> SendPipe) : base(Name) { this.SendPipe = SendPipe; }

        public override void Send(IEnumerable<CanFrame> Frames)
        {
            SendPipe.Send(Frames.ToList());
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