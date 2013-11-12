using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Communications.Can.Tests
{
    class CanFlowTest
    {
        class MockCanPort : CanPort
        {
            public MockCanPort() : base("Mock Can") { SendBuffer = new Queue<CanFrame>();}

            /// <summary>
            /// Получает или задаёт скорость порта (в бодах)
            /// </summary>
            public override int BaudRate { get; set; }

            /// <summary>
            /// Внутренняя реализация отправки сообщений
            /// </summary>
            /// <param name="Frames"></param>
            protected override void SendImplementation(IList<CanFrame> Frames)
            {
                foreach (var f in Frames)
                    SendBuffer.Enqueue(f);
            }

            public void PushFrames(params CanFrame[] Frames) { OnFramesRecieved(Frames); }

            public Queue<CanFrame> SendBuffer { get; private set; }
        }

        protected static IEnumerable<CanFrame> GetRandomFrames()
        {
            var r = new Random();
            while (true)
            {
                var data = new byte[r.Next(0, 8)];
                r.NextBytes(data);
                yield return CanFrame.NewWithId((UInt16) r.Next(0, 0x7ff), data);
            }
        }

        [Test]
        public void RecieveTest()
        {
            const int messagesCount = 10;
            const int perMessageDelay = 10;
            
            var testFrames = GetRandomFrames().Take(messagesCount).ToList();

            var mockPort = new MockCanPort();
            var flow = new CanFlow(mockPort, testFrames.Select(f => f.Descriptor).Distinct().ToArray());

            ThreadPool.QueueUserWorkItem(s =>
                                  {
                                      foreach (var frame in testFrames)
                                      {
                                          mockPort.PushFrames(frame);
                                          Thread.Sleep(perMessageDelay);
                                      }
                                  });

            Stopwatch watch = new Stopwatch();
            watch.Start();
            var readedFrames = flow.Read(TimeSpan.FromMilliseconds(perMessageDelay * 5), false).ToList();
            watch.Stop();

            Assert.LessOrEqual(watch.ElapsedMilliseconds, messagesCount*perMessageDelay*2, "Приём сообщений занял слишком много времени");
            Assert.AreEqual(testFrames.Count, readedFrames.Count, "Количество отправленых и принятых фреймов не совпадает");
            for (int i = 0; i < messagesCount; i++)
            {
                Assert.AreEqual(testFrames[i].Id, readedFrames[i].Id, "Порядок сообщений нарушен");
                Assert.IsTrue(testFrames[i].Data.SequenceEqual(readedFrames[i].Data), "Идентификатор сообщения не изменился, но данные исказились");
            }
        }
    }
}
