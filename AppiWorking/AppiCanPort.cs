using System.Collections.Generic;
using Communications.Can;

namespace Communications.Appi
{
    public class AppiCanPort : CanPort
    {
        private int _BaudRate;

        internal AppiCanPort(AppiDev Device, AppiLine Line)
            : base(Line.ToString())
        {
            this.Device = Device;
            this.Line = Line;
        }

        public AppiLine Line { get; private set; }

        private AppiDev Device { get; set; }

        public override int BaudRate
        {
            get { return _BaudRate; }
            set { Device.SetBaudRate(Line, value); }
        }

        protected override void SendImplementation(IList<CanFrame> Frames) { Device.SendFrames(Frames, Line); }

        internal void OnAppiFramesRecieved(IList<CanFrame> Frames) { OnFramesReceived(Frames); }

        internal void RenewBaudRate(int newValue)
        {
            if (newValue != _BaudRate)
            {
                _BaudRate = newValue;
                base.OnBaudRateChanged(newValue);
            }
        }
    }
}
