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
        private readonly USBPipe _readPipe;
        private readonly USBPipe _writePipe;

        public WinusbDevice(USBDeviceInfo deviceInfo, int ReadBufferSize = 4096)
            : this(
                String.Format("{0} - {1}", deviceInfo.Manufacturer, deviceInfo.DeviceDescription), deviceInfo,
                ReadBufferSize) { }

        public WinusbDevice(string Name, USBDeviceInfo deviceInfo, int ReadBufferSize = 4096) : base(Name)
        {
            try
            {
                this.ReadBufferSize = ReadBufferSize;
                var device = new USBDevice(deviceInfo);

                _readPipe = device.Pipes.First(p => p.IsIn);
                _readPipe.Policy.PipeTransferTimeout = 100;
                _readPipe.Policy.AutoClearStall = true;
                _readPipe.Flush();

                _writePipe = device.Pipes.First(p => p.IsOut);
                _writePipe.Policy.PipeTransferTimeout = 100;
            }
            catch (USBException e)
            {
                throw new WinusbCantOpenDeviceException(e);
            }
        }

        /// <summary>Размер буфера чтения</summary>
        public int ReadBufferSize { get; private set; }

        /// <summary>Производит запись в USB-порт</summary>
        /// <param name="Data">Последовательность Bulk-сообщений для записи</param>
        /// <param name="Timeout">Таймаут операции</param>
        public void Write(IEnumerable<UsbBulk> Data, TimeSpan Timeout)
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

        /// <summary>Производит чтение из USB-порта</summary>
        /// <param name="Timeout">Таймаут чтения</param>
        /// <returns>Последовательность прочитанных Bulk-сообщений</returns>
        public IEnumerable<UsbBulk> Read(TimeSpan Timeout)
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

        protected override IWinusbSocket InternalOpenSocket() { return new WinUsbSocket(Name, this); }
    }

    public interface IWinusbSocket : IUsbBulkSocket { }
}
