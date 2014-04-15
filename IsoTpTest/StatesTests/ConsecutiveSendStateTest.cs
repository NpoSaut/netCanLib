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

                Assert.IsInstanceOfType(connection.ConnectionState, typeof (ConsecutiveSendState), "Соединение переключилось в неподходящее состояние после отправки {0} кадра", i);

                Assert.AreEqual(transaction.Index, i + 1, "Неправильный индекс сообщения в транзакции");

                Assert.AreEqual(connection.SentFrames.Count, 1, "Должно было отправиться только одно сообщение");
                Assert.IsInstanceOfType(connection.SentFrames.First(), typeof (ConsecutiveFrame), "Отправлено сообщение не того типа");
                var frame = (ConsecutiveFrame)connection.SentFrames.Dequeue();
                Assert.AreEqual(frame.Index, i, "Отправилось сообщение с неправильным индексом");
                Assert.IsTrue(transaction.Data.Skip(i * consecutivePayload)
                                         .Take(consecutivePayload)
                                         .SequenceEqual(frame.Data), "Данные повредились при отправке");
                Assert.AreEqual(transaction.Position, (i + 1) * consecutivePayload, "Курсор в транзакции указывает на неправильное положение");
            }

            // Отправляем последнее сообщение из блока
            state.Operate(TimeSpan.MaxValue);
            Assert.IsInstanceOfType(connection.ConnectionState, typeof (WaitForControlFrameState),
                                    "Соединение переключилось в неправильное состояние после отправки блока");
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
            Assert.AreEqual(transaction.Done, true, "Транзакция не была отмечена как завершённая");
            Assert.IsInstanceOfType(connection.FinishedTransaction, typeof(TpSendTransaction), "В соединении не установилась завершённая транзакция");
        }
    }
}
