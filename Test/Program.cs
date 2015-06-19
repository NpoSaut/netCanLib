using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Communications.Appi.Devices;
using Communications.Appi.Factories;
using Communications.Can;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Frames;
using ReactiveWinUsb;

namespace Test
{
    internal class Program
    {
        private const int D1 = 0x1448;
        private const int D2 = 0x1488;
        private static readonly byte[] _data;

        private static readonly IDictionary<int, ConsoleColor> _printColors =
            new Dictionary<int, ConsoleColor>
            {
                { D1, ConsoleColor.Cyan },
                { D2, ConsoleColor.Yellow }
            };

        private static readonly EventLoopScheduler _consoleScheduler;

        static Program()
        {
            _data = Enumerable.Range(0, 1000).Select(i => (byte)i).ToArray();
            _consoleScheduler = new EventLoopScheduler();
        }

        private static void Main(string[] args)
        {
            var appiFactory = new AppiBlockFactory(new WinUsbFacade());
            IAppiDeviceInfo deviceInfo = appiFactory.EnumerateDevices().First();

            using (AppiDevice<AppiLine> appi = appiFactory.OpenDevice(deviceInfo))
            {
                ICanPort port = appi.CanPorts[AppiLine.Can1];

                port.Rx
                    .Where(f => f.Descriptor == D1 || f.Descriptor == D2)
                    .TimeInterval()
                    .SubscribeOn(_consoleScheduler)
                    .Subscribe(PrintCanFrame);

                Console.WriteLine("Starting Receive...");
                port.Rx
                    .IsoTpReceive(port.Tx, D1, D2, 10)
                    .SubscribeOn(_consoleScheduler)
                    .Subscribe(Print);
                Console.WriteLine("Receiving started.");

                if (args.Any(p => p.ToLower() == "s"))
                {
                    Console.WriteLine("Creating Sender...");
                    var transaction = new IsoTpTransmitTransaction();
                    IObservable<IsoTpFrame> isotpRx = port.Rx
                                                          .Where(f => f.Descriptor == D2)
                                                          .Select(f => IsoTpFrame.ParsePacket(f.Data));
                    IObserver<IsoTpFrame> isoTpTx = Observer.Create<IsoTpFrame>(frame => port.Tx.OnNext(frame.GetCanFrame(D1)));

                    _consoleScheduler.Schedule(() => Console.WriteLine("Sending Packet..."));
                    transaction.Begin(new IsoTpPacket(_data), isotpRx, isoTpTx, TimeSpan.FromMilliseconds(2000));
                    _consoleScheduler.Schedule(() => Console.WriteLine("Packet Sent!"));
                    Console.ReadLine();
                }

                Console.WriteLine("press enter to exit...");
                Console.ReadLine();
            }
        }

        private static void PrintCanFrame(TimeInterval<CanFrame> CanFrameTI)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("{0:ss\\.fff}: ", CanFrameTI.Interval);
            Console.ForegroundColor = _printColors[CanFrameTI.Value.Descriptor];
            Console.WriteLine(CanFrameTI.Value);
            Console.ResetColor();
        }

        private static void Print(IsoTpPacket Packet)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("ISO-TP Packet ({0} Bytes) Received", Packet.Data.Length);
            Console.ResetColor();
            //            Console.WriteLine(BitConverter.ToString(Packet.Data));

            Console.ForegroundColor = _data.SequenceEqual(Packet.Data)
                                          ? ConsoleColor.Green
                                          : ConsoleColor.DarkYellow;
            Console.WriteLine("Data is {0}", _data.SequenceEqual(Packet.Data) ? "OK" : "not ok");
            Console.ResetColor();

            Console.WriteLine();
        }
    }
}
