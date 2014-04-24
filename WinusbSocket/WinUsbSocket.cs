using System;
using System.Collections.Generic;
using Communications.Exceptions;
using Communications.Sockets;
using Communications.Usb;

namespace WinusbSocket
{
    public class WinUsbSocket : SocketBase<UsbBulk>, IWinusbSocket
    {
        public WinusbDevice Device { get; private set; }

        public WinUsbSocket(string Name, WinusbDevice Device) : base(Name) { this.Device = Device; }

        /// <summary>Отправляет дейтаграммы в сокет</summary>
        public override void Send(IEnumerable<UsbBulk> Data) { Device.Write(Data); }

        /// <summary>Выполняет чтение из сокета</summary>
        /// <param name="Timeout">Таймаут чтения</param>
        /// <param name="ThrowExceptionOnTimeout">
        ///     Указывает, следует ли выбрасывать исключение
        ///     <see cref="SocketTimeoutException" /> при превышении таймаута чтения, или просто прервать считывание
        ///     последовательности
        /// </param>
        /// <returns>Последовательность считанных дейтаграмм</returns>
        public override IEnumerable<UsbBulk> Receive(TimeSpan Timeout = new TimeSpan(), bool ThrowExceptionOnTimeout = false)
        {
            return Device.Read(Timeout, ThrowExceptionOnTimeout);
        }
    }
}