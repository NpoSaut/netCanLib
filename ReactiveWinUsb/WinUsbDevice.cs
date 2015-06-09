using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Usb;
using MadWizard.WinUSBNet;

namespace ReactiveWinUsb
{
    public class WinUsbDevice : IUsbDevice
    {
        private readonly byte[] _buffer;
        private readonly USBDevice _device;
        private readonly USBPipe _readPipe;
        private readonly EventLoopScheduler _scheduler;
        private readonly ConcurrentBag<IDisposable> _senders = new ConcurrentBag<IDisposable>();
        private readonly Subject<UsbFrame> _tx;
        private readonly USBPipe _writePipe;

        public WinUsbDevice(USBDevice Device, int BufferSize)
        {
            _device = Device;

            _buffer = new byte[BufferSize];

            _readPipe = _device.Pipes.First(p => p.IsIn);
            _readPipe.Policy.PipeTransferTimeout = 100;
            _readPipe.Policy.AutoClearStall = true;

            _writePipe = _device.Pipes.First(p => p.IsOut);
            _writePipe.Policy.PipeTransferTimeout = 100;

            _scheduler = new EventLoopScheduler();

            Rx = Observable.Interval(TimeSpan.Zero, _scheduler)
                           .Select(x => Read());

            _tx = new Subject<UsbFrame>();
            _tx.SubscribeOn(_scheduler).Subscribe(Write);
        }

        public IObserver<UsbFrame> Tx
        {
            get { return _tx; }
        }

        public IObservable<UsbFrame> Rx { get; private set; }

        /// <summary>
        ///     Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых
        ///     ресурсов.
        /// </summary>
        public void Dispose()
        {
            _scheduler.Schedule(() =>
                                {
                                    _scheduler.Dispose();
                                    _device.Dispose();
                                });
            foreach (IDisposable sender in _senders.ToArray())
                sender.Dispose();
        }

        private void Write(UsbFrame UsbFrame) { _writePipe.Write(UsbFrame.Data); }

        private UsbFrame Read()
        {
            int readSize = _readPipe.Read(_buffer);
            var newBuffer = new byte[readSize];
            Buffer.BlockCopy(_buffer, 0, newBuffer, 0, readSize);
            return new UsbFrame(newBuffer);
        }
    }
}
