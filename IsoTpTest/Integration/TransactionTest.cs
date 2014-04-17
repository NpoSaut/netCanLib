using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Communications.Can;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;
using CommunicationsTests.Stuff;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest.Integration
{
    [TestClass]
    public class IntegrationTransactionTest
    {
        protected readonly Random Rnd = new Random();

        protected Byte[] GetRandomBytes(int Count)
        {
            var res = new byte[Count];
            Rnd.NextBytes(res);
            return res;
        }

        protected IsoTpFrame TakeFrame(ConcurrentTestIsoTpConnection connection, TimeSpan Timeout)
        {
            IsoTpFrame frame = null;
            SpinWait.SpinUntil(() => connection.OutputQueue.TryDequeue(out frame), Timeout);
            return frame;
        }

        [TestMethod]
        public void CrossTransmitTest()
        {
            var connections = PairedConnection.Builder.Build();
            var sender = connections[0];
            var receiver = connections[1];

            var data = GetRandomBytes(Math.Min(sender.MaximumDatagramLength, receiver.MaximumDatagramLength));

            var receiverTask = Task.Run(() => receiver.Receive(TimeSpan.MaxValue));

            sender.Send(data, TimeSpan.MaxValue);

            receiverTask.Wait();
            var receivedData = receiverTask.Result;

            Assert.IsTrue(receivedData.SequenceEqual(data), "Данные были нарушены в ходе передачи");
        }

        [TestMethod]
        public void ManualShortTransactionReceiveTest()
        {
            var receiver = new ConcurrentTestIsoTpConnection(128, 0);
            var data = GetRandomBytes(SingleFrame.GetPayload(receiver.SubframeLength));
            
            var receiveTask = Task.Run(() => receiver.Receive(TimeSpan.FromSeconds(1)));

            Thread.Sleep(10);
            Assert.IsInstanceOfType(receiver.ConnectionState, typeof(ReadyToReceiveState), "Неверное состояние до получения какого-либо фрейма");

            receiver.InputQueue.Enqueue(new SingleFrame(data));

            receiveTask.Wait();

            Assert.IsTrue(receiveTask.Result.SequenceEqual(data), "Данные были повреждены при передаче");
        }

        [TestMethod]
        public void ManualLongTransactionReceiveTest()
        {
            const int blockSize = 3;
            var separationTime = TimeSpan.Zero;

            var receiver = new ConcurrentTestIsoTpConnection(blockSize, 0);
            int firstFramePayload = FirstFrame.GetPayload(receiver.SubframeLength);
            int consecutiveFramePayload = SingleFrame.GetPayload(receiver.SubframeLength);
            var data = GetRandomBytes(firstFramePayload + (2 * blockSize - 1) * consecutiveFramePayload);
            var dataReader = new BinaryReader(new MemoryStream(data));

            var receiveTask = Task.Run(() => receiver.Receive(TimeSpan.FromSeconds(1)));

            Thread.Sleep(10);
            Assert.IsInstanceOfType(receiver.ConnectionState, typeof(ReadyToReceiveState), "Неверное состояние до получения какого-либо фрейма");

            receiver.InputQueue.Enqueue(new FirstFrame(dataReader.ReadBytes(firstFramePayload), data.Length));

            var firstFlowControlFrame = (FlowControlFrame)TakeFrame(receiver, TimeSpan.FromSeconds(1));
            Assert.AreEqual(FlowControlFlag.ClearToSend, firstFlowControlFrame.Flag);
            Assert.AreEqual(blockSize, firstFlowControlFrame.BlockSize);
            Assert.AreEqual(separationTime, firstFlowControlFrame.SeparationTime);

            Assert.IsInstanceOfType(receiver.ConnectionState, typeof(ConsecutiveReceiveState), "Неверное состояние после получения FirstFrame");

            byte index = 0;
            for (int i = 0; i < blockSize; i++)
                receiver.InputQueue.Enqueue(new ConsecutiveFrame(dataReader.ReadBytes(consecutiveFramePayload), index++));

            var secondFlowControlFrame = (FlowControlFrame)TakeFrame(receiver, TimeSpan.FromSeconds(1));
            Assert.AreEqual(FlowControlFlag.ClearToSend, secondFlowControlFrame.Flag);
            Assert.AreEqual(blockSize, secondFlowControlFrame.BlockSize);
            Assert.AreEqual(separationTime, secondFlowControlFrame.SeparationTime);

            for (int i = 0; i < blockSize-1; i++)
                receiver.InputQueue.Enqueue(new ConsecutiveFrame(dataReader.ReadBytes(consecutiveFramePayload), index++));

            receiveTask.Wait();

            Assert.IsTrue(receiveTask.Result.SequenceEqual(data), "Данные были повреждены при передаче");
        }

        [TestMethod]
        public void ManualLongTransactionWringIndexReceiveTest()
        {
            const int blockSize = 10;

            var receiver = new ConcurrentTestIsoTpConnection(blockSize, 0);
            int firstFramePayload = FirstFrame.GetPayload(receiver.SubframeLength);
            int consecutiveFramePayload = SingleFrame.GetPayload(receiver.SubframeLength);
            

            var receiveTask = Task.Run(() => receiver.Receive(TimeSpan.FromSeconds(1)));
            receiver.InputQueue.Enqueue(new FirstFrame(GetRandomBytes(firstFramePayload), 1024));

            TakeFrame(receiver, TimeSpan.FromSeconds(1));

            byte index = 0;
            for (int i = 0; i < blockSize - 5; i++)
                receiver.InputQueue.Enqueue(new ConsecutiveFrame(GetRandomBytes(consecutiveFramePayload), index++));
            
            receiver.InputQueue.Enqueue(new ConsecutiveFrame(GetRandomBytes(consecutiveFramePayload), index + 2));

            try
            {
                receiveTask.Wait();
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(1, e.InnerExceptions.Count);
                Assert.IsInstanceOfType(e.InnerExceptions.First(), typeof(IsoTpSequenceException));
                var exc = (IsoTpSequenceException)e.InnerExceptions.First();
                Assert.AreEqual(index + 1, exc.ExpectedIndex, "Ожидали сообщения с неправильным индексом");
                Assert.AreEqual(index + 2, exc.ReceivedIndex, "Получили сообщение не с таким неправильным индексом, который хотели бы");
                return;
            }

            Assert.Fail("Не было обнаружено нарушение последовательности кадров");
        }
    }
}
