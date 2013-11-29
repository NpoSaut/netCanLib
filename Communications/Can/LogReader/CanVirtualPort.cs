using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace Communications.Can.LogReader
{
    public abstract class CanVirtualPort : CanPort, IDisposable
    {
        private Timer ReadTimer { get; set; }

        protected CanVirtualPort(String Name)
            : base(Name)
        {
            ReadTimer = new Timer(1000);
            ReadTimer.Elapsed += ReadTimer_Elapsed;
        }

        /// <summary>
        /// Начинает чтение лога
        /// </summary>
        public void Start()
        {
            ReadTimer.Start();
        }
        /// <summary>
        /// Начинает чтение лога с заданным интервалом
        /// </summary>
        public void Start(double interval)
        {
            ReadTimer.Interval = interval;
            ReadTimer.Start();
        }

        protected abstract CanFrame ReadNextFrame();

        void ReadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var f = ReadNextFrame();
            if (f != null) (this as IReceivePipe<CanFrame>).ProcessReceived(new[] { f });
            else ReadTimer.Stop();
        }

        protected override void SendImplementation(IList<CanFrame> Frames)
        {
            foreach (var fr in Frames)
                Console.WriteLine("{0}  <<-  {1}", Name, fr);
        }

        public virtual void Dispose()
        {
            ReadTimer.Stop();
        }


        private int _baudRate;
        public override int BaudRate
        {
            get { return _baudRate; }
            set
            {
                if (_baudRate != value)
                {
                    _baudRate = value;
                    OnBaudRateChanged();
                }
            }
        }
    }
}
