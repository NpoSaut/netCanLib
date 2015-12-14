using System;
using System.Threading;
using System.Threading.Tasks;
using Communications.Transactions;
using NUnit.Framework;

namespace Communications.Tests
{
    [TestFixture]
    public class TransactionTests
    {
        private class TestTransaction : LongTransactionBase<string>
        {
            private readonly string _transactionPayload;
            public TestTransaction(string TransactionPayload) { _transactionPayload = TransactionPayload; }
            protected override string GetPayload() { return _transactionPayload; }
        }

        [Test(Description = "Exception по отмене ожидания")]
        public void CancellationTest()
        {
            const string data = "test";
            var transaction = new TestTransaction(data);
            var cts = new CancellationTokenSource();
            Task<string> task = Task.Factory.StartNew(() => transaction.Wait(cts.Token));
            SpinWait.SpinUntil(() => task.Status == TaskStatus.Running);
            cts.Cancel();
            var exc = Assert.Throws<AggregateException>(task.Wait);
            Assert.IsInstanceOf<OperationCanceledException>(exc.InnerException);
        }

        [Test(Description = "Проверяет правильность работы функции Wait() вызванной после того, как транзакция уже была завершена")]
        public void InstantTransactionTest()
        {
            const string data = "test";
            var transaction = new TestTransaction(data);
            transaction.Commit();
            string result = transaction.Wait();
            Assert.AreEqual(data, result);
        }

        [Test(Description = "Exception по таймауту")]
        public void TimeoutElapsedTest()
        {
            const string data = "test";
            var transaction = new TestTransaction(data);
            Assert.Throws<TimeoutException>(() => transaction.Wait(TimeSpan.FromMilliseconds(10)));
        }

        [Test(Description = "Ожидание завершения транзакции в другом потоке")]
        public void WaitForCompleatedTest()
        {
            const string data = "test";
            var transaction = new TestTransaction(data);
            Task<string> task = Task.Factory.StartNew(() => transaction.Wait());
            SpinWait.SpinUntil(() => task.Status == TaskStatus.Running);
            transaction.Commit();
            task.Wait();
            Assert.AreEqual(data, task.Result);
        }
    }
}
