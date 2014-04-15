using System;
using System.Linq;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest.StatesTests
{
    [TestClass]
    public class ConsecutiveSendStateTest : IsoTpStateTest
    {
        private const int BlockSize = 12;

        [TestMethod]
        public void SendConsecutiveFrameTest()
        {
            var connection = new TestIsoTpConnection();
            var transaction = new TpSendTransaction(GetRandomBytes(1024))
                              {
                                  BlockSize = BlockSize,
                                  SeparationTime = TimeSpan.Zero,
                                  Index = 0
                              };

            int consecutivePayload = ConsecutiveFrame.GetPayload(connection.SubframeLength);

            var state = new ConsecutiveSendState(connection, transaction);
            connection.SetNextState(state);

            for (int i = 0; i < BlockSize - 1; i++)
            {
                state.Operate(TimeSpan.MaxValue);

                Assert.IsInstanceOfType(connection.ConnectionState, typeof (ConsecutiveSendState), "���������� ������������� � ������������ ��������� ����� �������� {0} �����", i);

                Assert.AreEqual(transaction.Index, i + 1, "������������ ������ ��������� � ����������");

                Assert.AreEqual(connection.SentFrames.Count, 1, "������ ���� ����������� ������ ���� ���������");
                Assert.IsInstanceOfType(connection.SentFrames.First(), typeof (ConsecutiveFrame), "���������� ��������� �� ���� ����");
                var frame = (ConsecutiveFrame)connection.SentFrames.Dequeue();
                Assert.AreEqual(frame.Index, i, "����������� ��������� � ������������ ��������");
                Assert.IsTrue(transaction.Data.Skip(i * consecutivePayload)
                                         .Take(consecutivePayload)
                                         .SequenceEqual(frame.Data), "������ ����������� ��� ��������");
                Assert.AreEqual(transaction.Position, (i + 1) * consecutivePayload, "������ � ���������� ��������� �� ������������ ���������");
            }

            // ���������� ��������� ��������� �� �����
            state.Operate(TimeSpan.MaxValue);
            Assert.IsInstanceOfType(connection.ConnectionState, typeof (WaitForControlFrameState),
                                    "���������� ������������� � ������������ ��������� ����� �������� �����");
        }

        [TestMethod]
        public void FinishingTransactionTest()
        {
            var connection = new TestIsoTpConnection();
            int consecutivePayload = ConsecutiveFrame.GetPayload(connection.SubframeLength);
            var transaction = new TpSendTransaction(GetRandomBytes(consecutivePayload * 5))
            {
                BlockSize = consecutivePayload * 10,
                SeparationTime = TimeSpan.Zero,
                Index = 0
            };

            var state = new ConsecutiveSendState(connection, transaction);
            connection.SetNextState(state);

            for (int i = 0; i < 5; i++)
            {
                state.Operate(TimeSpan.MaxValue);
            }
            Assert.AreEqual(transaction.Done, true, "���������� �� ���� �������� ��� �����������");
            Assert.IsInstanceOfType(connection.FinishedTransaction, typeof(TpSendTransaction), "� ���������� �� ������������ ����������� ����������");
        }
    }
}
