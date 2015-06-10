using System;
using System.Collections.Generic;

namespace Communications.Usb
{
    public interface IUsbFacade
    {
        IEnumerable<IUsbSlot> EnumerateDevices(String DeviceGuid);
    }
}