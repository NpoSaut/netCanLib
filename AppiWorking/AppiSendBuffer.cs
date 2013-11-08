using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Communications.Appi
{
    class AppiSendBuffer
    {
        private AppiDev Device { get; set; }
        public int OutgoingMessagesInBuffer { get; set; }
        public object Locker { get; private set; }

        public AppiSendBuffer(AppiDev Device)
        {
            this.Device = Device;
            Locker = new object();
        }

        public void PostCount(ushort OutMessagesInA)
        {
            OutgoingMessagesInBuffer = OutMessagesInA;
            lock (Locker)
            {
                if (OutgoingMessagesInBuffer <= 40)
                {
                    Monitor.Pulse(Locker);
                }
            }
        }
    }
}
