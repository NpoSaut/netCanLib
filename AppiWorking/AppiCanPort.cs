using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Appi
{
    public class AppiCanPort : CanPort
    {
        public AppiLine Line { get; private set; }

        private AppiDev Device { get; set; }

        internal AppiCanPort(AppiDev Device, AppiLine Line)
            : base(Line.ToString())
        {
            this.Device = Device;
            this.Line = Line;
        }

        protected override void SendImplementation(IList<CanFrame> Frames)
        {
            Device.SendFrames(Frames, Line);
        }

        protected override ISocket<CanFrame> CreateSocket() { return new AppiCanSocket(string.Format("Appi{0}", Line), this); }

        private int _baudRate;
        public override int BaudRate
        {
            get { return _baudRate; }
            set
            {
                Device.SetBaudRate(this.Line, value);
            }
        }
        internal void RenewBaudRate(int newValue)
        {
            if (newValue != _baudRate)
            {
                _baudRate = newValue;
                base.OnBaudRateChanged();
            }
        }
    }
}
