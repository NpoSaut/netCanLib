using System;
using System.Linq;
using Communications.Appi.Devices;
using Communications.Appi.Factories;
using Communications.Can;

namespace Communications.Appi
{
    public class AppiDeviceCanPortProvider<TLineKey> : ICanPortProvider where TLineKey : struct, IConvertible
    {
        private readonly IAppiFactory<TLineKey> _appiDeviceFactory;
        private readonly Lazy<AppiDevice<TLineKey>> _deviceLazy;
        private readonly TLineKey _line;

        public AppiDeviceCanPortProvider(IAppiFactory<TLineKey> AppiDeviceFactory, TLineKey Line)
        {
            _appiDeviceFactory = AppiDeviceFactory;
            _deviceLazy = new Lazy<AppiDevice<TLineKey>>(() => _appiDeviceFactory.OpenDevice(_appiDeviceFactory.EnumerateDevices().First(d => d.IsFree)));
            _line = Line;
        }

        public ICanPort OpenPort() { return _deviceLazy.Value.CanPorts[_line]; }
    }
}
