using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Communications.Appi;
using Communications.Appi.Devices;
using Communications.Appi.Factories;
using Communications.Can;
using MadWizard.WinUSBNet;
using ReactiveWinUsb;

namespace Test
{
    internal class Program
    {
        private static readonly List<string> _deviceGuids =
            new List<string>
            {
                "524cc09a-0a72-4d06-980e-afee3131196e",
                "3af3f480-41b5-4c24-b2a9-6aacf7de3d01"
            };

        private static void Main(string[] args)
        {
            USBDeviceInfo d = _deviceGuids.SelectMany(USBDevice.GetDevices).First();

            var appiFactory = new AppiBlockFactory();
            using (AppiDevice<AppiLine> appi = appiFactory.OpenDevice(new WinUsbDeviceSlot(new USBDevice(d), 2048)))
            {
                appi.CanPorts[AppiLine.Can1].Rx
                                            .Where(f => f.Descriptor == 0x1888)
                                            .Select(f => CanFrame.NewWithDescriptor(0x1088, f.Data))
                                            .Do(Console.WriteLine)
                                            .Subscribe(appi.CanPorts[AppiLine.Can1].Tx);
                //.Subscribe(Console.WriteLine);

                Console.ReadLine();
            }
        }
    }
}
