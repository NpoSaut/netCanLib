using System.Collections.Generic;
using System.Linq;
using Communications.Usb;
using MadWizard.WinUSBNet;

namespace ReactiveWinUsb
{
    public class WinUsbFacade : IUsbFacade
    {
        public IEnumerable<IUsbSlot> EnumerateDevices(string DeviceGuid)
        {
            return USBDevice.GetDevices(DeviceGuid)
                            .Select(dev => new WinUsbDeviceSlot(dev));
        }
    }
}
