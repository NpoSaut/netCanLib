using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Communications.Appi.Devices;
using Communications.Appi.Factories;
using Communications.Can;
using ReactiveWinUsb;

namespace Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var appiFactory = new AppiBlockFactory(new WinUsbFacade());
            IAppiDeviceInfo deviceInfo = appiFactory.EnumerateDevices().First();

            using (AppiDevice<AppiLine> appi = appiFactory.OpenDevice(deviceInfo))
            {
                IConnectableObservable<CanFrame> inputData = appi.CanPorts[AppiLine.Can1].Rx
                                                                                         .Where(f => f.Descriptor == 0x1888)
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
