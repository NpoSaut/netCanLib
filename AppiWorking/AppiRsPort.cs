using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Serial;

namespace Communications.Appi
{
    public class AppiRsPort : RsPort
    {
        private AppiDev Device { get; set; }

        internal AppiRsPort(AppiDev Device, String Name)
            : base(Name)
        {
            this.Device = Device;
        }

        public override int BaudRate
        {
            get { return 9600; }
            set { throw new NotImplementedException("Изменение скорости RS-485 линии не реализовано даже на стороне АППИ"); }
        }

        protected override void SendImplementation(IList<byte> Data) { Device.PushSerialData(Data.ToArray()); }
        protected override ISocket<byte> CreateSocket() { throw new NotImplementedException(); }
    }
}
