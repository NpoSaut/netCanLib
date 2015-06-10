using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
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
            //            IObservable<long> data = Observable.Interval(TimeSpan.FromMilliseconds(100))
            //                                               .Take(12)
            //                                               .Do(x => Console.WriteLine("in {0}", x));
            //
            //            IObservable<int> limitations = Observable.Interval(TimeSpan.FromMilliseconds(1000)).Select(i => (int)(i))
            //                                                     .Do(x => Console.WriteLine("limit {0}", x));
            //
            //            IObservable<IList<long>> output = data.Limit(limitations);
            //
            //            output.Subscribe(x => Console.WriteLine("Output {0} elements: {1}", x.Count, string.Join(", ", x)));
            //            Console.ReadLine();
            //            return;

            USBDeviceInfo d = _deviceGuids.SelectMany(USBDevice.GetDevices).First();

            var appiFactory = new AppiBlockFactory();
            using (AppiDevice<AppiLine> appi = appiFactory.OpenDevice(new WinUsbDeviceSlot(new USBDevice(d), 2048)))
            {
                IConnectableObservable<CanFrame> inputData = appi.CanPorts[AppiLine.Can1].Rx
                                                                                         .Where(f => f.Descriptor == 0x1088)
                                                                                         .Publish();

                inputData.Select((x, i) => new { x, i }).Subscribe(x => Console.WriteLine("{0}: {1}", x.i + 1, x.x));

                //                inputData.SelectMany(f => Enumerable.Range(0, 80).Select(i => CanFrame.NewWithDescriptor(0x1088, BitConverter.GetBytes(i))))
                //                         .Subscribe(appi.CanPorts[AppiLine.Can1].Tx);

                inputData.Connect();

                Console.ReadLine();

                Task.Factory.StartNew(() => Observable.Interval(TimeSpan.Zero)
                                                      .Take(8000)
                                                      .Select(i => CanFrame.NewWithDescriptor(0x1088, BitConverter.GetBytes(i)))
                                                      .Subscribe(appi.CanPorts[AppiLine.Can1].Tx));

                Console.ReadLine();
            }
        }
    }
}
