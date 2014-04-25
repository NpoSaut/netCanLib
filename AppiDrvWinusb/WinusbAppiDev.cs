using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Appi.Exceptions;
using MadWizard.WinUSBNet;

namespace Communications.Appi.Winusb
{
    /// <summary>
    /// Связь с АППИ через WinUSB
    /// </summary>
    public class WinusbAppiDev : AppiDev
    {
        /// <summary>
        /// GUID устройства
        /// </summary>
        public static readonly List<string> DeviceGuids =
            new List<string>
                                                          {
            "524cc09a-0a72-4d06-980e-afee3131196e",
            "3af3f480-41b5-4c24-b2a9-6aacf7de3d01"
        };

        private String DevicePath { get; set; }
        /// <summary>
        /// USB-устройство
        /// </summary>
        private USBDevice Device { get; set; }
        /// <summary>
        /// Труба чтения
        /// </summary>
        private USBPipe ReadPipe { get; set; }
        /// <summary>
        /// Труба записи
        /// </summary>
        private USBPipe WritePipe { get; set; }

        /// <summary>
        /// Перечисляет все слоты подключённых устройств АППИ
        /// </summary>
        public static IEnumerable<AppiDeviceSlot> GetDevices()
        {
            return DeviceGuids.SelectMany(DeviceGuid =>
                USBDevice.GetDevices(DeviceGuid).Select(di => new WinusbAppiDeviceSlot(di)).Cast<AppiDeviceSlot>());
        }

        internal WinusbAppiDev(USBDeviceInfo di)
        {
            // TODO: Отлавливать исключения WinUsbNet
            try
            {
            Device = new USBDevice(di);
            DevicePath = di.DevicePath;
            
            ReadPipe = Device.Pipes.First(p => p.IsIn);
            ReadPipe.Policy.PipeTransferTimeout = 100;
            ReadPipe.Policy.AutoClearStall = true;
            ReadPipe.Flush();
            
            WritePipe = Device.Pipes.First(p => p.IsOut);
            WritePipe.Policy.PipeTransferTimeout = 100;

            lock (OpenedDevicesLocker)
            {
                OpenedDevices.Add(this);
            }
        }
            catch (USBException e)
            {
                throw new AppiConnectoinException(e);
            }
        }

        #region Чтение и запись буфера
        /// <summary>
        /// Чтение буфера
        /// </summary>
        protected override byte[] ReadBufferImplement()
        {
            try
            {
                Byte[] buff = new Byte[BufferSize];
                ReadPipe.Read(buff);
                //return buff.SkipWhile(b => b == 0).ToArray();
                return buff;
            }
            catch (Exception usbExc)
            {
                OnDisconnected();
                throw new AppiConnectoinException("Ошибка при чтении буфера АППИ из USB", usbExc);
            }
        }
        /// <summary>
        /// Запись буфера
        /// </summary>
        /// <param name="Buffer">Данные для записи</param>
        protected override void WriteBufferImplement(byte[] Buffer)
        {
            try
            {
                WritePipe.Write(Buffer);
            }
            catch (USBException usbExc)
            {
                OnDisconnected();
                throw new AppiConnectoinException("Ошибка при записи буфера АППИ в USB", usbExc);
            }
        } 
        #endregion

        private static readonly object OpenedDevicesLocker = new object();
        private static List<WinusbAppiDev> OpenedDevices { get; set; }
        
        static WinusbAppiDev()
        {
            OpenedDevices = new List<WinusbAppiDev>();
        }

        public override void Dispose()
        {
            lock (OpenedDevicesLocker)
            {
                OpenedDevices.Remove(this);
            }
            base.Dispose();
            Device.Dispose();
        }

        internal static bool IsDeviceOpened(string DevicePath)
        {
            lock (OpenedDevicesLocker)
            {
                return OpenedDevices.Any(d => d.DevicePath == DevicePath);
            }
        }
    }
}
