﻿using System;
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
        private readonly Subject<UsbFrame> _tx;
        private readonly USBPipe _writePipe;

        private ConcurrentBag<IDisposable> _senders;

        public WinUsbDevice(USBDevice Device, int BufferSize)
        {
            _device = Device;

            _buffer = new byte[BufferSize];
            
            _readPipe = _device.Pipes.First(p => p.IsIn);
            _readPipe.Policy.PipeTransferTimeout = 100;
            _readPipe.Policy.AutoClearStall = true;

            _writePipe = _device.Pipes.First(p => p.IsOut);
            _writePipe.Policy.PipeTransferTimeout = 100;

            var scheduler = new EventLoopScheduler();

            Rx = Observable.Interval(TimeSpan.Zero, scheduler)
                           .Select(x => Read());

            _tx = new Subject<UsbFrame>();
            _tx.SubscribeOn(scheduler).Subscribe(Write);
        }

        public IObservable<UsbFrame> Rx { get; private set; }

        /// <summary>
        ///     Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых
        ///     ресурсов.
        /// </summary>
        public void Dispose()
        {
            _device.Dispose();
            foreach (IDisposable sender in _senders.ToArray())
                sender.Dispose();
        }

        public void Tx(IObservable<UsbFrame> Flow) { _senders.Add(Flow.Subscribe(_tx)); }

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
