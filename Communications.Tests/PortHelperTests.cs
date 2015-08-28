using System;
using System.Linq;
using System.Reactive.Linq;
using Communications.PortHelpers;
using NUnit.Framework;

namespace Communications.Tests
{
    [TestFixture]
    public class PortHelperTests
    {
        [Test]
        public void SimpleLoopbackRequestTest()
        {
            var port = new TestPort<string>(r => string.Format("ans_{0}", r), true);
            string ans = port.Request("123");
            Assert.AreEqual("ans_123", ans);
        }

        [Test]
        public void SimpleRequestTest()
        {
            var port = new TestPort<string>(r => string.Format("ans_{0}", r), false);
            string ans = port.Request("123");
            Assert.AreEqual("ans_123", ans);
        }

        //[Test]
        //[TestCase(0, 10)]
        //[TestCase(10, 30)]
        //[TestCase(30, 50)]
        ////[TestCase(30, 10)] TODO: Разобраться с тем, как использовать тут экцепшены
        //public void TimeoutRequestTest(int Delay, int Timeout)
        //{
        //    var port = new TestPort<string>(flow => flow.Throttle(TimeSpan.FromMilliseconds(Delay))
        //                                                .Select(r => string.Format("ans_{0}", r)), false);
        //    string ans = port.Request("123", TimeSpan.FromMilliseconds(Timeout));
        //    Assert.AreEqual("ans_123", ans);
        //}
    }
}
