using Communications.Usb;
using MadWizard.WinUSBNet;

namespace ReactiveWinUsb
{
    public class WinUsbDeviceSlot : IUsbSlot
    {
        private readonly int _bufferSize;
        private readonly USBDevice _device;

        public WinUsbDeviceSlot(USBDevice Device, int BufferSize)
        {
            _device = Device;
            _bufferSize = BufferSize;
        }

        public IUsbDevice OpenDevice() { return new WinUsbDevice(_device, _bufferSize); }
    }
}
