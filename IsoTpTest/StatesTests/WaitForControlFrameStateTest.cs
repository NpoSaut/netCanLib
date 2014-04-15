using System;
using System.Runtime.InteropServices;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest.StatesTests
{
    [TestClass]
    public class WaitForControlFrameStateTest : IsoTpStateTest
    {
        private const byte BlockSize = 150;
        private static readonly TimeSpan SeparationTime = TimeSpan.Zero;

        [TestMethod]
        public void ReceiveClearToSendTest()
        {
            var connection = new TestIsoTpConnection();
            var transaction = new TpSendTransaction(new byte[12]);

            connection.IncomingQueue.Enqueue(new FlowControlFrame(FlowControlFlag.ClearToSend, BlockSize, SeparationTime));

            var state = new WaitForControlFrameState(connection, transaction);
            connection.SetNextState(state);
            state.Operate(TimeSpan.MaxValue);

            Assert.AreEqual(connection.SentFrames.Count, 0);
            Assert.IsInstanceOfType(connection.ConnectionState, typeof(ConsecutiveSendState));
            Assert.AreEqual(transaction.BlockSize, BlockSize);
            Assert.AreEqual(transaction.SeparationTime, SeparationTime);
            Assert.AreEqual(transaction.Index, 0);
        }

        [TestMethod]
        public void ReceiveWaitTest()
        {
            var connection = new TestIsoTpConnection();
            var transaction = new TpSendTransaction(new byte[12]);

            connection.IncomingQueue.Enqueue(new FlowControlFrame(FlowControlFlag.Wait, BlockSize, SeparationTime));

            var state = new WaitForControlFrameState(connection, transaction);
            connection.SetNextState(state);
            state.Operate(TimeSpan.MaxValue);

            Assert.AreEqual(connection.SentFrames.Count, 0);
            Assert.IsInstanceOfType(connection.ConnectionState, typeof(WaitForControlFrameState));
        }

        [TestMethod]
        public void ReceiveAbortTest()
        {
            var connection = new TestIsoTpConnection();
            var transaction = new TpSendTransaction(new byte[12]);

            connection.IncomingQueue.Enqueue(new FlowControlFrame(FlowControlFlag.Abort, BlockSize, SeparationTime));

            var state = new WaitForControlFrameState(connection, transaction);
            connection.SetNextState(state);
            try
            {
                state.Operate(TimeSpan.MaxValue);
            }
            catch (IsoTpTransactionAbortedException)
            {
                return;
            }

            Assert.Fail("Не пробросилось исключение при приёме FlowControl.Abort");
        }

        private void ReceiveWrongFrameTest(IsoTpFrame WrongFrame)
        {
            var connection = new TestIsoTpConnection();
            var transaction = new TpSendTransaction(new byte[12]);

            connection.IncomingQueue.Enqueue(WrongFrame);

            var state = new WaitForControlFrameState(connection, transaction);
            connection.SetNextState(state);
            try
            {
                state.Operate(TimeSpan.MaxValue);
            }
            catch (IsoTpWrongFrameException)
            {
                return;
            }

            Assert.Fail("Не пробросилось исключение при приёме неправильного фрейма");
        }

        [TestMethod]
        public void WrongFramesTest()
        {
            ReceiveWrongFrameTest(new ConsecutiveFrame(new byte[2], 12));
            ReceiveWrongFrameTest(new FirstFrame(new byte[6], 123));
            ReceiveWrongFrameTest(new SingleFrame(new byte[2]));
        }
    }
}