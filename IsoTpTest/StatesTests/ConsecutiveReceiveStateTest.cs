using System;
using System.Linq;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest.StatesTests
{
    [TestClass]
    public class ConsecutiveReceiveStateTest : IsoTpStateTest
    {
        [TestMethod]
        public void NormalReceiveTest()
        {
            const int blockLength = 5;

            var connection = new TestIsoTpConnection(BlockLength: blockLength);
            var consecutivePayload = ConsecutiveFrame.GetPayload(connection.SubframeLength);
            var data = GetRandomBytes(consecutivePayload * blockLength);

            var transaction = new TpReceiveTransaction(data.Length);

            var state = new ConsecutiveReceiveState(connection, transaction);
            connection.SetNextState(state);

            int i;
            for (i = 0; i < blockLength - 1; i++)
            {
                connection.IncomingQueue.Enqueue(new ConsecutiveFrame(data.Skip(i * consecutivePayload).Take(consecutivePayload).ToArray(), i));
                state.Operate(TimeSpan.MaxValue);
                Assert.AreEqual(transaction.Position, (i + 1) * consecutivePayload, "������ ���������� ��������� �� ������������ �������");
                Assert.AreEqual(transaction.ExpectedFrameIndex, i + 1, "���������� ������� ������������ ������ ���������");

                Assert.IsInstanceOfType(connection.ConnectionState, typeof(ConsecutiveReceiveState), "���������� ��������� � �������� ��������� ����� ����� {0} �����", i);
            }
            connection.IncomingQueue.Enqueue(new ConsecutiveFrame(data.Skip(i * consecutivePayload).Take(consecutivePayload).ToArray(), i));
            state.Operate(TimeSpan.MaxValue);
            Assert.IsInstanceOfType(connection.ConnectionState, typeof(SendControlFrameState), "���������� �� ������������� � ��������� �������� ControlFrame ����� ���������� �������� �����");

            Assert.AreEqual(transaction.Done, true, "���������� �� ���������� ��� �����������");
            Assert.IsInstanceOfType(connection.FinishedTransaction, typeof(TpReceiveTransaction), "���������� �� ���� ��������� ����������� ����������");
            Assert.IsTrue(transaction.Data.SequenceEqual(data), "������ ���� ���������� ��� ��������");
        }

        [TestMethod]
        public void InvalidIndexReceiveTest()
        {
            const int blockLength = 5;

            var connection = new TestIsoTpConnection(BlockLength: blockLength);
            var consecutivePayload = ConsecutiveFrame.GetPayload(connection.SubframeLength);
            var data = GetRandomBytes(consecutivePayload * blockLength);

            var transaction = new TpReceiveTransaction(data.Length);

            var state = new ConsecutiveReceiveState(connection, transaction);
            connection.SetNextState(state);
            connection.IncomingQueue.Enqueue(new ConsecutiveFrame(GetRandomBytes(consecutivePayload), 1));

            try
            {
                state.Operate(TimeSpan.MaxValue);
            }
            catch (IsoTpSequenceException)
            {
                return;
            }

            Assert.Fail("�� ���� ��������� ����������, ��������� �� ������ � ������������������");
        }
    }
}