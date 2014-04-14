using System;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest.StatesTests
{
    [TestClass]
    public class ReadyToReceiveStateTest : IsoTpStateTest
    {
        [TestMethod]
        public void TestFrameReceive()
        {
            
            var state = new ReadyToReceiveState();
        }
    }
}
