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
                Assert.AreEqual(transaction.Position, (i + 1) * consecutivePayload,
                                "Курсор транзакции указывает на неправильную позицию");
                Assert.AreEqual(transaction.ExpectedFrameIndex, i + 1,
                                "Транзакция ожидает неправильный индекс сообщения");

                Assert.IsInstanceOfType(connection.ConnectionState, typeof (ConsecutiveReceiveState),
                                        "Соединение оказалось в неверном состоянии после приёма {0} кадра", i);
            }
            connection.IncomingQueue.Enqueue(
                                             new ConsecutiveFrame(
                                                 data.Skip(i * consecutivePayload).Take(consecutivePayload).ToArray(), i));
            state.Operate(TimeSpan.MaxValue);
            Assert.IsInstanceOfType(connection.ConnectionState, typeof (SendControlFrameState),
                                    "Соединение не переключилось в состояние отправки ControlFrame после завершения отправки блока");

            Assert.AreEqual(transaction.Done, true, "Транзакция не пометилась как завершённая");
            Assert.IsInstanceOfType(connection.FinishedTransaction, typeof (TpReceiveTransaction),
                                    "Соединению не была присвоена завершённая транзакция");
            Assert.IsTrue(transaction.Data.SequenceEqual(data), "Данные были повреждены при передаче");
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
                //                Assert.AreEqual(connection.SentFrames.Count, 1, "Должно было быть отправлено сообщение с отменой транзакции");
                //                Assert.IsInstanceOfType(connection.SentFrames.Peek(), typeof(FlowControlFrame), "Отправлено сообщение неправильного типа");
                //                var cancelFrame = (FlowControlFrame)connection.SentFrames.Dequeue();
                //                Assert.AreEqual(cancelFrame.Flag, FlowControlFlag.Abort, "Отправленное FlowControl сообщение должно было отменить транзакцию");
                //                
                //                Assert.IsInstanceOfType(connection.ConnectionState, typeof(ReadyToReceiveState), "Соединение должно было перейти в состояние ожидания подключения");
                return;
            }

            Assert.Fail("Не было выброшено исключение, говорящее об ошибке в последовательности");
        }
    }
}
