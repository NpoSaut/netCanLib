using System;
using Communications.Usb;

namespace Communications.Appi.Factories
{
    public interface IAppiDeviceInfo
    {
        IUsbSlot UsbSlot { get; }
        Boolean IsFree { get; }
    }
}