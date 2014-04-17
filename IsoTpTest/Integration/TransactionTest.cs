using System;
using System.Linq;
using System.Threading.Tasks;
using Communications.Can;
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

        [TestMethod]
        public void SendReceiveTest()
        {
            var connections = PairedConnection.Builder.Build();
            var sender = connections[0];
            var receiver = connections[1];

            var data = GetRandomBytes(Math.Min(sender.MaximumDatagramLength, receiver.MaximumDatagramLength));

            var receiverTask = Task.Run(() => receiver.Receive(TimeSpan.MaxValue));

            sender.Send(data, TimeSpan.MaxValue);

            receiverTask.Wait();
            var receivedData = receiverTask.Result;

            Assert.IsTrue(receivedData.SequenceEqual(data));
        }
    }
}
