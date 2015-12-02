namespace Communications.Appi.Encoders
{
    public class AppiSetBaudRateBuffer<TLineKey>
    {
        public AppiSetBaudRateBuffer(int BaudRate, TLineKey Interface)
        {
            this.BaudRate = BaudRate;
            this.Interface = Interface;
        }

        public int BaudRate { get; private set; }
        public TLineKey Interface { get; private set; }
    }
}