using System;
using System.Threading.Tasks;
using Communications.Can;
using Communications.Protocols.IsoTP;
using CommunicationsTests.Stuff;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest.Integration
{
    [TestClass]
    public class IntegrationTransactionTest
    {
        private static Random r = new Random();

        [TestMethod]
        public void SendTest()
        {
            const int dataLength = 4000;

            UInt16 descriptorA = CanFrame.GetDescriptorFor(r.Next(0, CanFrame.IdMaxValue), 8);
            UInt16 descriptorB;
            do descriptorB = CanFrame.GetDescriptorFor(r.Next(0, CanFrame.IdMaxValue), 8);
            while (descriptorB == descriptorA);

            Byte[] data = new byte[dataLength];
            r.NextBytes(data);

            var flows = ChainedCanFlow.Take(2, descriptorA, descriptorB);

//            var recieveAction =
//                Task<TpReceiveTransaction>.Factory.StartNew(() => IsoTp.Receive(flows[0], descriptorA, descriptorB));
//
//            IsoTp.Send(flows[1], descriptorA, descriptorB, data);
//
//            recieveAction.Wait(1000);
//            CollectionAssert.AreEqual(data, recieveAction.Result.Data, "Данные были повреждены в ходе передачи");
        }
    }
}
