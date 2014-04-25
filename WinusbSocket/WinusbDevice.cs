using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Ports;
using Communications.Usb;
using MadWizard.WinUSBNet;
using WinusbSocket.Exceptions;

namespace WinusbSocket
{
    /// <summary>Устройство на WinUSB</summary>
    public class WinusbDevice : SingleSocketPortBase<IWinusbSocket, UsbBulk>
    {
        private readonly USBDeviceInfo _deviceInfo;
        private readonly int _readBufferSize;

        public WinusbDevice(USBDeviceInfo deviceInfo, int ReadBufferSize = 4096)
            : this(
                String.Format("{0} - {1}", deviceInfo.Manufacturer, deviceInfo.DeviceDescription), deviceInfo,
                ReadBufferSize) { }

        public WinusbDevice(string Name, USBDeviceInfo deviceInfo, int ReadBufferSize = 4096) : base(Name)
        {
            _deviceInfo = deviceInfo;
            _readBufferSize = ReadBufferSize;
        }

        protected override IWinusbSocket InternalOpenSocket()
        {
            var device = new USBDevice(_deviceInfo);
            return new WinUsbSocket(Name, device, _readBufferSize);
        }

        public static IEnumerable<WinusbDevice> GetDevices(Guid DeviceGuid) { return USBDevice.GetDevices(DeviceGuid).Select(di => new WinusbDevice(di)); }
    }

    public interface IWinusbSocket : IUsbBulkSocket { }
}
