using System;
using Communications.Can;

namespace Communications.Appi.Ports
{
    public class AppiCanPortOptions : CanPortOptions
    {
        private int _baudRate;

        /// <summary>Создаёт новые опции порта с поддержкой Loopback и указанным <see cref="ILoopbackInspector{TFrame}" />
        /// </summary>
        public AppiCanPortOptions() : base(new CanPortLoopbackInspector()) { _baudRate = 0; }

        /// <summary>Скорость обмена</summary>
        public override int BaudRate
        {
            get { return _baudRate; }
            set { OnBaudRateChanged(new BaudRateChangedEventArgs(value)); }
        }

        internal event EventHandler<BaudRateChangedEventArgs> RequestChangeBaudRate;

        protected virtual void OnBaudRateChanged(BaudRateChangedEventArgs e)
        {
            EventHandler<BaudRateChangedEventArgs> handler = RequestChangeBaudRate;
            if (handler != null) handler(this, e);
        }

        public void UpdateBitrate(int NewBaudRate) { _baudRate = NewBaudRate; }
    }
}
