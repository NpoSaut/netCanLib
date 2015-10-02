using System;
using Communications.Appi.Devices;
using Communications.Can;

namespace Communications.Appi
{
    public class AppiDeviceCanPortProvider<TLineKey> : ICanPortProvider where TLineKey : struct, IConvertible
    {
        private readonly AppiDevice<TLineKey> _appiDevice;
        private readonly TLineKey _line;

        public AppiDeviceCanPortProvider(AppiDevice<TLineKey> AppiDevice, TLineKey Line)
        {
            _appiDevice = AppiDevice;
            _line = Line;
        }

        public ICanPort OpenPort() { return _appiDevice.CanPorts[_line]; }
    }
}
