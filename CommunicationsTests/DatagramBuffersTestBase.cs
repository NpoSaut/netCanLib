using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Communications.Exceptions;
using Communications.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunicationsTests
{
    [TestClass]
    public abstract class DatagramBuffersTestBase
    {
        protected abstract IDatagramBuffer<Byte> CreateBuffer();

        protected Tuple<Byte[], Byte[]> CheckBufferRoutine(IDatagramBuffer<Byte> Buffer, TimeSpan PushDelay, TimeSpan ReadTimeout,
                                          bool ThrowException, int DataLength = 20)
        {
            return CheckBufferRoutine(Buffer, PushDelay, ReadTimeout, ThrowException, (i, t) => t, DataLength);
        }

        protected Tuple<Byte[], Byte[]> CheckBufferRoutine(IDatagramBuffer<Byte> Buffer, TimeSpan PushDelay, TimeSpan ReadTimeout,
                                          bool ThrowException, Func<int, TimeSpan, TimeSpan> PushingDelaySelector,
                                          int DataLength = 20)
        {
            var r = new Random();
            var testData = new byte[DataLength];
            r.NextBytes(testData);

            Task.Factory.StartNew(() =>
                                  {
                                      int i = 0;
                                      foreach (var b in testData)
                                      {
                                          Buffer.Enqueue(new[] { b });
                                          Thread.Sleep(PushingDelaySelector(i, PushDelay));
                                          i++;
                                      }
                                  });
            var processedData = Buffer.Read(ReadTimeout, ThrowException).ToArray();
            return new Tuple<byte[], byte[]>(testData, processedData);
        }

        [TestMethod]
        public void CheckBufferPushAndPops()
        {
            var res = CheckBufferRoutine(CreateBuffer(), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20), false);
            Assert.IsTrue(res.Item1.SequenceEqual(res.Item2), "Данные были искажены после помещения в буфер");
        }

        [TestMethod]
        public void CheckTimeoutWithNoException()
        {
            var res = CheckBufferRoutine(CreateBuffer(),
                                         TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20), false,
                                         (i, t) => i < 9 ? t : TimeSpan.FromMilliseconds(4 * t.TotalMilliseconds), 20);
            Assert.AreEqual(10, res.Item2.Length, "Количество элементов не соответствует ожидаемому");
        }

        [TestMethod]
        public void CheckTimeoutWithException()
        {
            try
            {
                CheckBufferRoutine(CreateBuffer(),
                                        TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20), true,
                                        (i, t) => i < 9 ? t : TimeSpan.FromMilliseconds(4 * t.TotalMilliseconds), 20);
                Assert.Fail("Не было выдано никакого исключения");
            }
            catch (SocketReadTimeoutException sre)
            {
                return;
            }
            catch (Exception)
            {
                Assert.Fail("Было выдано не правильное исключение");
            }
        }
    }

    [TestClass]
    public class ConcurrendDatagramBufferTestBase : DatagramBuffersTestBase
    {
        protected override IDatagramBuffer<byte> CreateBuffer() { return new ConcurrentDatagramBuffer<byte>(); }
    }
}