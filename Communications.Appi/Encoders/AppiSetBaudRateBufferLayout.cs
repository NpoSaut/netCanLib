namespace Communications.Appi.Encoders
{
    public class AppiSetBaudRateBufferLayout
    {
        public AppiSetBaudRateBufferLayout(int InterfaceIndexOffset, int CommandCodeOffset, int BaudRateValueOffset)
        {
            this.InterfaceIndexOffset = InterfaceIndexOffset;
            this.CommandCodeOffset = CommandCodeOffset;
            this.BaudRateValueOffset = BaudRateValueOffset;
        }

        public int InterfaceIndexOffset { get; private set; }
        public int CommandCodeOffset { get; private set; }
        public int BaudRateValueOffset { get; private set; }
    }
}
