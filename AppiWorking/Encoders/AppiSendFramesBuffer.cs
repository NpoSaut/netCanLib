using System.Collections.Generic;
using Communications.Can;

namespace Communications.Appi.Encoders
{
    public class AppiSendFramesBuffer<TLineKey>
    {
        public AppiSendFramesBuffer(TLineKey Interface, ICollection<CanFrame> Frames)
        {
            this.Interface = Interface;
            this.Frames = Frames;
        }

        public AppiSendFramesBuffer(TLineKey Interface, params CanFrame[] Frames) : this(Interface, (ICollection<CanFrame>)Frames) { }

        public ICollection<CanFrame> Frames { get; private set; }
        public TLineKey Interface { get; private set; }
    }
}