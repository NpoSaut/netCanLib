using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Exceptions;
using Communications.Sockets;
using Communications.Usb;
using MadWizard.WinUSBNet;
using WinusbSocket.Exceptions;

namespace WinusbSocket
{
    public class WinUsbSocket : SocketBase<UsbBulk>, IWinusbSocket
    {
        private readonly USBPipe _readPipe;
        private readonly USBPipe _writePipe;

        /// <summary>Размер буфера чтения</summary>
        public int ReadBufferSize { get; private set; }

        internal WinUsbSocket(string Name, USBDevice Device, int ReadBufferSize) : base(Name)
        {
            this.ReadBufferSize = ReadBufferSize;
            try
            {
                _readPipe = Device.Pipes.First(p => p.IsIn);
                _readPipe.Policy.PipeTransferTimeout = 100;
                _readPipe.Policy.AutoClearStall = true;
                _readPipe.Flush();

                _writePipe = Device.Pipes.First(p => p.IsOut);
                _writePipe.Policy.PipeTransferTimeout = 100;
            }
            catch (USBException e)
            {
                throw new WinusbCantOpenDeviceException(e);
            }
        }

        /// <summary>Отправляет дейтаграммы в сокет</summary>
        public override void Send(IEnumerable<UsbBulk> Data, TimeSpan Timeout = default(TimeSpan))
        {
            _writePipe.Policy.PipeTransferTimeout = (int)Timeout.TotalMilliseconds;
            try
            {
                foreach (UsbBulk bulk in Data)
                    _writePipe.Write(bulk.Data);
            }
            catch (USBException usbExc)
            {
                throw new WinusbPortException("Ошибка при записи Bulk-буфера USB", usbExc);
            }
        }

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
            _readPipe.Policy.PipeTransferTimeout = (int)Timeout.TotalMilliseconds;
            var buff = new Byte[ReadBufferSize];
            while (true)
            {
                int readLength;
                try
                {
                    readLength = _readPipe.Read(buff);
                }
                catch (USBException usbExc)
                {
                    throw new WinusbPortException("Ошибка при чтении Bulk-буфера из USB", usbExc);
                }
                yield return new UsbBulk(buff.Take(readLength).ToArray());
            }
        }
    }
}