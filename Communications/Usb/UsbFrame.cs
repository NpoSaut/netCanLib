namespace Communications.Usb
{
    /// <summary>USB-пакет</summary>
    public class UsbFrame : IDataFrame
    {
        public UsbFrame(byte[] Data) { this.Data = Data; }

        /// <summary>Данные, содержащиеся в USB-пакете</summary>
        public byte[] Data { get; private set; }
    }
}
