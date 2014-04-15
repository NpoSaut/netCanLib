using System;
using System.Linq;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest.StatesTests
{
    [TestClass]
    public class BeginTransmitionStateTest : IsoTpStateTest
    {
        [TestMethod]
        public void TestSingleFrame()
        {
            var connection = new TestIsoTpConnection();
            var data = GetRandomBytes(connection.SubframeLength - 5);
            
            var state = new BeginTransmitionState(connection, data);
            state.Operate(TimeSpan.MaxValue);
            
            Assert.AreEqual(1, connection.SentFrames.Count, "Должно было отправиться только одно сообщение");
            var frame = connection.SentFrames.Dequeue();
            Assert.IsInstanceOfType(frame, typeof(SingleFrame), "Должно было отправиться SingleFrame сообщение");
            Assert.AreEqual(data, ((SingleFrame)frame).Data, "Данные исказились при передаче");
        }

        [TestMethod]
        public void TestBeginTransaction()
        {
            var connection = new TestIsoTpConnection();
            var data = GetRandomBytes(connection.SubframeLength * 5);

            var state = new BeginTransmitionState(connection, data);
            state.Operate(TimeSpan.MaxValue);

            Assert.IsInstanceOfType(connection.ConnectionState, typeof(WaitForControlFrameState), "Установлено неправильное состояние соединения");

            Assert.AreEqual(1, connection.SentFrames.Count, "Должно было отправиться только одно сообщение");
            var frame = connection.SentFrames.Dequeue();
            Assert.IsInstanceOfType(frame, typeof(FirstFrame), "Должно было отправиться FirstFrame сообщение");
            Assert.AreEqual(data.Length, ((FirstFrame)frame).PacketSize);
            Assert.IsTrue(data.Take(FirstFrame.GetPayload(connection.SubframeLength)).SequenceEqual(((FirstFrame)frame).Data), "Данные исказились при передаче");
        }
    }
}