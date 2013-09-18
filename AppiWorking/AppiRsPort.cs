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

        private object bufferLocker = new object();
        private Queue<Byte> inBuffer { get; set; }

        internal AppiRsPort(AppiDev Device, String Name)
            : base(Name)
        {
            this.Device = Device;
            this.inBuffer = new Queue<byte>();
        }

        protected override byte[] ReadBufferImplementation()
        {
            lock (bufferLocker)
            {
                var res = inBuffer.ToArray();
                inBuffer.Clear();
                return res;
            }
        }
        protected override void WriteBufferImplementation(byte[] buff)
        {
            Device.PushSerialData(buff);
        }

        internal void OnAppiRsBufferRead(Byte[] buff)
        {
            lock (bufferLocker)
            {
                foreach (var b in buff)
                    inBuffer.Enqueue(b);
            }
        }
    }
}
