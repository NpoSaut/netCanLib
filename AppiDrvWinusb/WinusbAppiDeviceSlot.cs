﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MadWizard.WinUSBNet;

namespace Communications.Appi.Winusb
{
    public class WinusbAppiDeviceSlot : AppiDeviceSlot
    {
        public USBDeviceInfo DeviceInfo { get; private set; }

        public WinusbAppiDeviceSlot(USBDeviceInfo OnUsbDeviceInfo)
        {
            DeviceInfo = OnUsbDeviceInfo;
        }

        public override bool IsFree
        {
            get { return !WinusbAppiDev.IsDeviceOpened(DeviceInfo.DevicePath); }
        }

        protected override AppiDev InternalOpenDevice()
        {
            var dev = new WinusbAppiDev(DeviceInfo);
            return dev;
        }
    }
}
