using System;
using Communications.Appi.Devices;
using Communications.Usb;

namespace Communications.Appi.Factories
{
    public interface IAppiFactory<TLineKey> where TLineKey : struct, IConvertible
    {
        AppiDevice<TLineKey> OpenDevice(IUsbSlot Slot);
    }
}