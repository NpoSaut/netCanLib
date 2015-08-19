using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Can;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Frames;
using log4net;
using log4net.Config;
using NUnit.Framework;
using Rhino.Mocks;

namespace IsoTpTest
{
    [TestFixture]
    public class IntegrationTests
    {
        public IntegrationTests()
        {
            BasicConfigurator.Configure();
            _loger = LogManager.GetLogger(GetType());
        }

        private const int BlockSize = 5;

        private readonly ILog _loger;

        private Tuple<IIsoTpConnection, IIsoTpConnection> GetConnections()
        {
            var canSubject = new Subject<CanFrame>();
            canSubject.Select(f => IsoTpFrame.ParsePacket(f.Data)).Subscribe(f => _loger.DebugFormat("FUDP: {0}", f));

            var can = MockRepository.GenerateMock<ICanPort>();
            can.Stub(x => x.Rx).Return(canSubject);
            can.Stub(x => x.Tx).Return(canSubject);
            can.Stub(x => x.Options).Return(new CanPortOptions());

            return Tuple.Create<IIsoTpConnection, IIsoTpConnection>(
                new IsoTpOverCanPort(can, 0x28, 0x48, new IsoTpConnectionParameters(BlockSize)),
                new IsoTpOverCanPort(can, 0x48, 0x28, new IsoTpConnectionParameters(BlockSize)));
        }

        [Test]
        [TestCase(5, Description = "Транзакция в одну посылку")]
        [TestCase(10, Description = "Транзакция в несколько посылок")]
        [TestCase(4095, Description = "Транзакция настолько длинная, чтобы потребовалось запросить FlowControl")]
        public void SingleTransaction(int TransactionLength)
        {
            _loger.Info("Test Started");

            //var data = new byte[TransactionLength];
            //(new Random()).NextBytes(data);

            byte[] data = Enumerable.Repeat((byte)0xaa, 6)
                                    .Concat(Enumerable.Range(0, int.MaxValue)
                                                      .SelectMany(block =>
                                                                  Enumerable.Range(1, BlockSize)
                                                                            .SelectMany(i =>
                                                                                        new byte[] { (byte)block, 0, 0, 0, 0, 0, (byte)i })))
                                    .Take(TransactionLength)
                                    .ToArray();

            Tuple<IIsoTpConnection, IIsoTpConnection> x = GetConnections();
            IConnectableObservable<IsoTpPacket> replay = x.Item2.Rx.Replay();
            using (replay.Connect())
            {
                x.Item1.Tx.OnNext(new IsoTpPacket(data));
                byte[] incomData = replay.First().Data;

                CollectionAssert.AreEqual(data, incomData);
            }
        }
    }
}
