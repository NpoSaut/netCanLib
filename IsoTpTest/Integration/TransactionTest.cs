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
            //Assert.Fail("Тест загублен!");
            const int dataLength = 4000;

            var sockets = CanBrother.TakeBrothers(2);

            #region Подбор дескрипторов
            UInt16 descriptorA = CanFrame.GetDescriptorFor(r.Next(0, CanFrame.IdMaxValue), 8);
            UInt16 descriptorB;
            do descriptorB = CanFrame.GetDescriptorFor(r.Next(0, CanFrame.IdMaxValue), 8);
            while (descriptorB == descriptorA);
            #endregion

            Byte[] data = new byte[dataLength];
            r.NextBytes(data);

            var receiveAction =
                Task<TpReceiveTransaction>.Factory.StartNew(() => IsoTp.Receive(sockets[0], descriptorA, descriptorB));

            IsoTp.Send(sockets[1], descriptorA, descriptorB, data);

            receiveAction.Wait(1000);
            CollectionAssert.AreEqual(data, receiveAction.Result.Data, "Данные были повреждены в ходе передачи");
        }
    }
}
