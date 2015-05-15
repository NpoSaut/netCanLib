using System;
using Communications.Appi.Devices;

namespace Communications.Appi.Factories
{
    public interface IAppiFactory<TLineKey> where TLineKey : struct, IConvertible
    {
        AppiDevice<TLineKey> OpenDevice(IUsbSlot Slot);
    }
}