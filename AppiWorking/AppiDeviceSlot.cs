﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Appi.Exceptions;

namespace Communications.Appi
{
    /// <summary>
    /// Место подключения к АППИ
    /// </summary>
    public abstract class AppiDeviceSlot
    {
        /// <summary>
        /// Проверяет, занято ли устройство
        /// </summary>
        public abstract bool IsFree { get; }

        object openingLocker = new object();
        /// <summary>
        /// Открывает АППИ
        /// </summary>
        /// <param name="BeginListening">Начинает прослушивать линию сразу же после открытия устройства</param>
        /// <returns></returns>
        public AppiDev OpenDevice(bool BeginListening = true)
        {
            lock (openingLocker)
            {
                if (IsFree)
                {
                    var dev = InternalOpenDevice();
                    if (dev != null)
                    {
                        dev.Initialize();
                        OpenedDevice = dev;
                        return dev;
                    }
                    else throw new AppiException("Функция открытия устройства вернула null");
                }
                else throw new DeviceSlotAlreadyOpenedException(this);
            }
        }
        protected abstract AppiDev InternalOpenDevice();

        /// <summary>
        /// Уже открытое устройство на этом слоте (null, если устройство не открыто)
        /// </summary>
        public AppiDev OpenedDevice { get; private set; }
        /// <summary>
        /// Имеется ли открытое устройство на этом слоте
        /// </summary>
        public bool IsOpened { get;  private set; }
    }

    /// <summary>
    /// Исключение возникает в том случае, если слот устройства уже был открыт в другом месте
    /// </summary>
    public class DeviceSlotAlreadyOpenedException : AppiException
    {
        public AppiDeviceSlot Slot { get; set; }

        public DeviceSlotAlreadyOpenedException(AppiDeviceSlot slot)
            : base(string.Format("Устройство {0} уже было открыто", slot))
        {
            this.Slot = slot;
        }
    }
}
