using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CommunicationsTests.Stuff;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunicationsTests
{
    [TestClass]
    public class CanFlowTest
    {
        private class BufferImpl
        {
        }

        [TestMethod]
        public void RecieveTest()
        {
//            const int messagesCount = 10;
//            const int perMessageDelay = 10;
//            
//            var testFrames = CanFrameHelper.GetRandomFrames().Take(messagesCount).ToList();
//
//            var mockPort = new MockCanPort();
//            var flow = new CanFlow(mockPort, testFrames.Select(f => f.Descriptor).Distinct().ToArray());
//
//            ThreadPool.QueueUserWorkItem(s =>
//                                  {
//                                      foreach (var frame in testFrames)
//                                      {
//                                          mockPort.PushFrames(frame);
//                                          Thread.Sleep(perMessageDelay);
//                                      }
//                                  });
//
//            var watch = new Stopwatch();
//            watch.Start();
//            var readedFrames = flow.Read(TimeSpan.FromMilliseconds(perMessageDelay * 5), false).ToList();
//            watch.Stop();
//
//            const double targetTime = messagesCount*perMessageDelay*2;
//            Assert.IsTrue(watch.ElapsedMilliseconds <= targetTime, "Приём сообщений занял слишком много времени: {0}мс вместо {1}мс", watch.ElapsedMilliseconds, targetTime);
//            Assert.AreEqual(testFrames.Count, readedFrames.Count, "Количество отправленых и принятых фреймов не совпадает");
//            for (int i = 0; i < messagesCount; i++)
//            {
//                Assert.AreEqual(testFrames[i].Id, readedFrames[i].Id, "Порядок сообщений нарушен");
//                Assert.IsTrue(testFrames[i].Data.SequenceEqual(readedFrames[i].Data), "Идентификатор сообщения не изменился, но данные исказились");
//            }
        }
    }
}
