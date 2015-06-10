using Communications.Usb;
using MadWizard.WinUSBNet;

namespace ReactiveWinUsb
{
    public class WinUsbDeviceSlot : IUsbSlot
    {
        private readonly USBDeviceInfo _deviceInfo;

        public WinUsbDeviceSlot(USBDeviceInfo DeviceInfo) { _deviceInfo = DeviceInfo; }

        public IUsbDevice OpenDevice(int BufferSize) { return new WinUsbDevice(new USBDevice(_deviceInfo), BufferSize); }
    }
}
