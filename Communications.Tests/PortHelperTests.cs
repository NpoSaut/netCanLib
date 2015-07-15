using System.Diagnostics;
using System.Reactive.Linq;
using NUnit.Framework;

namespace Communications.Tests
{
    [TestFixture]
    public class PortHelperTests
    {
        [Test]
        public void SimpleRequestTest()
        {
            var port = new TestPort<string>(r => string.Format("ans_{0}", r), false);
            var ans = port.Request("123");
            Assert.AreEqual("ans_123", ans);
        }

        [Test]
        public void SimpleLoopbackRequestTest()
        {
            var port = new TestPort<string>(r => string.Format("ans_{0}", r), true);
            var ans = port.Request("123");
            Assert.AreEqual("ans_123", ans);
        }
    }
}
