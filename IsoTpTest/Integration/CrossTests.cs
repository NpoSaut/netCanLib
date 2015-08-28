using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Communications.Can;
using Communications.PortHelpers;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Frames;
using Communications.Transactions;
using log4net;
using log4net.Config;
using NUnit.Framework;
using Rhino.Mocks;

namespace IsoTpTest.Integration
{
    [TestFixture]
    public class CrossTests
    {
        public CrossTests()
        {
            BasicConfigurator.Configure();
            _loger = LogManager.GetLogger(GetType());
        }

        private const int BlockSize = 15;

        private readonly ILog _loger;

        private Tuple<IsoTpOverCanPort, IsoTpOverCanPort, ICanPort> GetConnections()
        {
            var canSubject = new Subject<CanFrame>();
            IObservable<InstantaneousTransaction<CanFrame>> canTransactions =
                canSubject.Select(f => new InstantaneousTransaction<CanFrame>(f)).Publish().RefCount();

            canSubject.Select(f => IsoTpFrame.ParsePacket(f.Data))
                      .Subscribe(f => _loger.DebugFormat("FUDP: {0}         THREAD: {1}", f, Thread.CurrentThread.Name));

            var can = MockRepository.GenerateMock<ICanPort>();
            can.Stub(x => x.Rx).Return(canTransactions);
            can.Stub(x => x.BeginSend(Arg<CanFrame>.Is.Anything))
               .Return(null)
               .WhenCalled(x =>
                           {
                               canSubject.OnNext((CanFrame)x.Arguments[0]);
                               x.ReturnValue = new InstantaneousTransaction<CanFrame>((CanFrame)x.Arguments[0]);
                           });
            can.Stub(x => x.Options).Return(new CanPortOptions());


            return Tuple.Create(
                new IsoTpOverCanPort(can, 0x28, 0x48, "Port A", new IsoTpConnectionParameters(BlockSize)),
                new IsoTpOverCanPort(can, 0x48, 0x28, "Port B", new IsoTpConnectionParameters(BlockSize)),
                can);
        }

        private static byte[] GetData(int DataLength, byte filler)
        {
            return Enumerable.Repeat((byte)0xaa, 6)
                             .Concat(Enumerable.Range(0, int.MaxValue)
                                               .SelectMany(block =>
                                                           Enumerable.Range(1, BlockSize)
                                                                     .SelectMany(i =>
                                                                                 new byte[] { (byte)block, 0, filler, filler, filler, 0, (byte)i })))
                             .Take(DataLength)
                             .ToArray();
        }

        [Test(Description = "Отправка нескольких посылок из порта A в порт B")]
        [TestCase(5, 5, 5, Description = "Три SingleFrame транзакции")]
        [TestCase(35, 35, 35, Description = "Три Short транзакции")]
        [TestCase(1000, 2000, 2000, Description = "Три Long транзакции")]
        public void MultipleOneWayTransactionTest(params int[] TransactionLengths)
        {
            List<byte[]> dataSamples = TransactionLengths.Select((length, i) => GetData(length, (byte)(i + 1))).ToList();

            Tuple<IsoTpOverCanPort, IsoTpOverCanPort, ICanPort> x = GetConnections();
            IConnectableObservable<IsoTpPacket> replay = x.Item2.Rx.WaitForTransactionCompleated().Replay();
            using (replay.Connect())
            {
                foreach (var dataSample in dataSamples)
                    x.Item1.BeginSend(new IsoTpPacket(dataSample)).Wait();

                IList<byte[]> incomDataSamples = replay.Take(TransactionLengths.Count())
                                                       .Select(packet => packet.Data)
                                                       .Do(data => _loger.InfoFormat("Packet Received {0}", BitConverter.ToString(data)))
                                                       .ToList()
                                                       .First();

                for (int i = 0; i < TransactionLengths.Count(); i++)
                    CollectionAssert.AreEqual(dataSamples[i], incomDataSamples[i]);
            }
        }

        [Test(Description = "Отправка одной посылки из порта A в порт B")]
        [TestCase(5, Description = "Транзакция в одну посылку")]
        [TestCase(10, Description = "Транзакция в несколько посылок")]
        [TestCase(4095, Description = "Транзакция настолько длинная, чтобы потребовалось запросить FlowControl")]
        public void OneWayTransactionTest(int TransactionLength)
        {
            byte[] data = GetData(TransactionLength, 0x00);

            Tuple<IsoTpOverCanPort, IsoTpOverCanPort, ICanPort> x = GetConnections();
            IConnectableObservable<IsoTpPacket> replay = x.Item2.Rx.WaitForTransactionCompleated().Replay();
            using (replay.Connect())
            {
                x.Item1.BeginSend(new IsoTpPacket(data));
                byte[] incomData = replay.First().Data;

                CollectionAssert.AreEqual(data, incomData);
            }
        }

        [Test(Description = "Ping-Pong отправка сообщений между двумя портами")]
        [TestCase(5, 5, Description = "Обмен SingleFrame-транзакциями")]
        [TestCase(5, 15, Description = "SingleFrame -> Short")]
        [TestCase(15, 5, Description = "Short -> SingleFrame")]
        [TestCase(15, 15, Description = "Short -> Short")]
        [TestCase(5, 1000, Description = "SingleFrame -> Long")]
        [TestCase(1000, 5, Description = "Long -> SingleFrame")]
        [TestCase(1000, 2000, Description = "Long -> Long")]
        public void PingPongTransactionTest(int Transaction1Length, int Transaction2Length)
        {
            byte[] data1 = GetData(Transaction1Length, 0xaa);
            byte[] data2 = GetData(Transaction2Length, 0xbb);
            byte[] receivedData1, receivedData2 = null;

            Tuple<IsoTpOverCanPort, IsoTpOverCanPort, ICanPort> x = GetConnections();
            IConnectableObservable<IsoTpPacket> replay = x.Item1.Rx.WaitForTransactionCompleated().Replay();

            // Making Loopback
            x.Item2.Rx
             .Subscribe(transaction =>
                        {
                            _loger.Info("---------------------- RECEIVING ---------------------");
                            transaction.Wait();
                            receivedData2 = transaction.Payload.Data;
                            _loger.Info("---------------------- RESPONSE ----------------------");
                            _loger.InfoFormat("                  {0}", BitConverter.ToString(receivedData2));
                            Task.Factory.StartNew(() => x.Item2.BeginSend(new IsoTpPacket(data2)));
                        });

            using (replay.Connect())
            {
                _loger.Info("                щас отправим");
                x.Item1.BeginSend(new IsoTpPacket(data1));

                receivedData1 = replay.Timeout(TimeSpan.FromSeconds(1)).First().Data;
            }

            Assert.AreEqual(data1, receivedData2, "Сообщение было искажено при передаче в прямую сторону");
            Assert.AreEqual(data2, receivedData1, "Сообщение было искажено при передаче в обратную сторону");
        }
    }
}
