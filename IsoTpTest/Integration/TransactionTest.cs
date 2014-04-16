using System;
using System.Threading.Tasks;
using Communications.Can;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Frames;
using CommunicationsTests.Stuff;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest.Integration
{
    [TestClass]
    public class IntegrationTransactionTest
    {
        private class PairedConnection : IsoTpConnectionBase
        {
            public override int SubframeLength
            {
                get { return 8; }
            }

            public override IsoTpFrame ReadNextFrame(TimeSpan Timeout) { throw new NotImplementedException(); }
            public override void SendFrame(IsoTpFrame Frame) { throw new NotImplementedException(); }
        }

        [TestMethod]
        public void SendReceiveTest()
        {
        }
    }
}
