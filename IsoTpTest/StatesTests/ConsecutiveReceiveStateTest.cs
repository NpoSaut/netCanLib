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
            int consecutivePayload = ConsecutiveFrame.GetPayload(connection.SubframeLength);
            byte[] data = GetRandomBytes(consecutivePayload * blockLength);

            var transaction = new TpReceiveTransaction(data.Length);

            var state = new ConsecutiveReceiveState(connection, transaction);
            connection.SetNextState(state);

            int i;
            for (i = 0; i < blockLength - 1; i++)
            {
                connection.IncomingQueue.Enqueue(
                                                 new ConsecutiveFrame(
                                                     data.Skip(i * consecutivePayload)
                                                         .Take(consecutivePayload)
                                                         .ToArray(), i));
                state.Operate(TimeSpan.MaxValue);
                Assert.AreEqual((i + 1) * consecutivePayload, transaction.Position, "������ ���������� ��������� �� ������������ �������");
                Assert.AreEqual(i + 1, transaction.ExpectedFrameIndex, "���������� ������� ������������ ������ ���������");

                Assert.IsInstanceOfType(connection.ConnectionState, typeof (ConsecutiveReceiveState), "���������� ��������� � �������� ��������� ����� ����� {0} �����", i);
            }
            connection.IncomingQueue.Enqueue(new ConsecutiveFrame(data.Skip(i * consecutivePayload).Take(consecutivePayload).ToArray(), i));
            state.Operate(TimeSpan.MaxValue);
            Assert.IsInstanceOfType(connection.ConnectionState, typeof (SendControlFrameState), "���������� �� ������������� � ��������� �������� ControlFrame ����� ���������� �������� �����");

            Assert.AreEqual(true, transaction.Done, "���������� �� ���������� ��� �����������");
            Assert.IsInstanceOfType(connection.FinishedTransaction, typeof (TpReceiveTransaction),
                                    "���������� �� ���� ��������� ����������� ����������");
            Assert.IsTrue(transaction.Data.SequenceEqual(data), "������ ���� ���������� ��� ��������");
        }

        [TestMethod]
        public void InvalidIndexReceiveTest()
        {
            const int blockLength = 5;

            var connection = new TestIsoTpConnection(BlockLength: blockLength);
            int consecutivePayload = ConsecutiveFrame.GetPayload(connection.SubframeLength);
            byte[] data = GetRandomBytes(consecutivePayload * blockLength);

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

        [TestMethod]
        public void AbortTransactionOnExceptionTest()
        {
            var connection = new TestIsoTpConnection();
            var transaction = new TpReceiveTransaction(1024);

            var state = new ConsecutiveReceiveState(connection, transaction);
            connection.SetNextState(state);

            state.OnException(new Exception());

            Assert.AreEqual(1, connection.SentFrames.Count, "������ ���� ����������� ��������� �� ������");
            Assert.IsInstanceOfType(connection.SentFrames.Peek(), typeof(FlowControlFrame), "�������� ��� ������������� ��������� �� ������ ����������");
            var frame = (FlowControlFrame)connection.SentFrames.Dequeue();
            Assert.AreEqual(FlowControlFlag.Abort, frame.Flag, "�������� ���� FlowControl ���������");
        }
    }
}
