using Communications.Usb;

namespace Communications.Appi.Factories
{
    public class AppiDeviceInfo : IAppiDeviceInfo
    {
        public AppiDeviceInfo(IUsbSlot UsbSlot) { this.UsbSlot = UsbSlot; }
        public IUsbSlot UsbSlot { get; private set; }

        public bool IsFree
        {
            get { return true; }
        }
    }
}