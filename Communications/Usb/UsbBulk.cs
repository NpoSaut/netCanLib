using System;

namespace Communications.Usb
{
    public class UsbBulk
    {
        public Byte[] Data { get; private set; }
        
        public UsbBulk(byte[] Data) { this.Data = Data; }
    }
}