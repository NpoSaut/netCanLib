using System;
using System.Collections.Generic;
using Communications.Appi.Devices;

namespace Communications.Appi.Factories
{
    public interface IAppiFactory<TLineKey> where TLineKey : struct, IConvertible
    {
        IEnumerable<IAppiDeviceInfo> EnumerateDevices();
        AppiDevice<TLineKey> OpenDevice(IAppiDeviceInfo DeviceInfo);
    }
}
