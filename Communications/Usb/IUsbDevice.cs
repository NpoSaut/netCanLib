using System;

namespace Communications.Usb
{
    public interface IUsbDevice : IDisposable
    {
        IObservable<UsbFrame> Rx { get; }
    }
}