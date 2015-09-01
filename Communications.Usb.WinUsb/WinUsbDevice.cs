using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Communications;
using Communications.Transactions;
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
            Options = new PortOptions<UsbFrame>();

            _device = Device;

            _buffer = new byte[BufferSize];

            _readPipe = _device.Pipes.First(p => p.IsIn);
            _readPipe.Policy.PipeTransferTimeout = 100;
            _readPipe.Policy.AutoClearStall = true;

            _writePipe = _device.Pipes.First(p => p.IsOut);
            _writePipe.Policy.PipeTransferTimeout = 100;

            _scheduler = new EventLoopScheduler(ts => new Thread(ts) { Name = "WinUSB Thread" });

            Rx = Observable.Interval(TimeSpan.Zero, _scheduler)
                           .Select(x => Read())
                           .Select(frame => new InstantaneousTransaction<UsbFrame>(frame));

            _tx = new Subject<UsbFrame>();
            _tx.SubscribeOn(_scheduler).Subscribe(Write);
        }

        /// <summary>Поток входящих сообщений</summary>
        public IObservable<ITransaction<UsbFrame>> Rx { get; private set; }

        public IObserver<UsbFrame> Tx
        {
            get { return _tx; }
        }

        public PortOptions<UsbFrame> Options { get; private set; }

        /// <summary>Начинает отправку кадра</summary>
        /// <param name="Frame">Кадр для отправки</param>
        /// <returns>Транзакция передачи</returns>
        public ITransaction<UsbFrame> BeginSend(UsbFrame Frame)
        {
            Write(Frame);
            return new InstantaneousTransaction<UsbFrame>(Frame);
        }

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
