using System;
using System.Linq;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest.StatesTests
{
    [TestClass]
    public class ReadyToReceiveStateTest : IsoTpStateTest
    {
        [TestMethod]
        public void TestReceiveShortTransaction()
        {
            var connection = new TestIsoTpConnection();
            int singleFramePayload = SingleFrame.GetPayload(connection.SubframeLength);
            byte[] data = GetRandomBytes(singleFramePayload);

            var state = new ReadyToReceiveState(connection);
            connection.SetNextState(state);

            connection.IncomingQueue.Enqueue(new SingleFrame(data));

            state.Operate(TimeSpan.MaxValue);

            Assert.AreEqual(0, connection.SentFrames.Count, "Не должно было отправиться ни одного пакета");
            Assert.IsInstanceOfType(connection.FinishedTransaction, typeof(TpReceiveTransaction), "Транзакция не была передана в соединение");
            Assert.AreEqual(true, connection.FinishedTransaction.Done, "Транзакция не была помечена как завершённая");
            Assert.IsTrue(connection.FinishedTransaction.Data.SequenceEqual(data), "Данные были повреждены при передаче");
        }

        [TestMethod]
        public void TestBeginReceiveLongTransaction()
        {
            var connection = new TestIsoTpConnection();
            int firstFramePayload = FirstFrame.GetPayload(connection.SubframeLength);
            byte[] data = GetRandomBytes(firstFramePayload * 2);

            var state = new ReadyToReceiveState(connection);
            connection.SetNextState(state);

            connection.IncomingQueue.Enqueue(new FirstFrame(data.Take(firstFramePayload).ToArray(), data.Length));

            state.Operate(TimeSpan.MaxValue);

            Assert.AreEqual(0, connection.SentFrames.Count, "Не должно было отправиться ни одного пакета");
            Assert.IsNull(connection.FinishedTransaction, "В соединении оказалась ненулевая завершённая транзакция");
            Assert.IsInstanceOfType(connection.ConnectionState, typeof(SendControlFrameState), "Неверное состояние соединения после принятия FirstFrame");

            var transaction = (TpReceiveTransaction)((SendControlFrameState)connection.ConnectionState).Transaction;
            Assert.AreEqual(false, transaction.Done, "Транзакция ошибочно пометилась как завершённая");
            Assert.AreEqual(data.Length, transaction.Length, "Неправильно определился размер транзакции");
            Assert.AreEqual(firstFramePayload, transaction.Position, "В транзакцию не были записаны данные из первого кадра");
            Assert.IsTrue(transaction.Data.Take(firstFramePayload).SequenceEqual(data.Take(firstFramePayload)), "Данные исказились при передаче");
        }
    }
}