using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Appi.Devices;
using Communications.Appi.Exceptions;
using Communications.Usb;

namespace Communications.Appi.Factories
{
    public abstract class AppiFactoryBase<TLineKey> : IAppiFactory<TLineKey> where TLineKey : struct, IConvertible
    {
        private readonly string _deviceGuid;
        private readonly IUsbFacade _usbFacade;

        protected AppiFactoryBase(string DeviceGuid, IUsbFacade UsbFacade)
        {
            _deviceGuid = DeviceGuid;
            _usbFacade = UsbFacade;
        }

        public IEnumerable<IAppiDeviceInfo> EnumerateDevices()
        {
            return _usbFacade.EnumerateDevices(_deviceGuid)
                             .Select(usbInfo => new AppiDeviceInfo(usbInfo));
        }

        public AppiDevice<TLineKey> OpenDevice(IAppiDeviceInfo DeviceInfo)
        {
            if (!DeviceInfo.IsFree)
                throw new AppiDeviceOccupiedException();
            return OpenDeviceImplementation(DeviceInfo);
        }

        protected abstract AppiDevice<TLineKey> OpenDeviceImplementation(IAppiDeviceInfo DeviceInfo);
    }
}
