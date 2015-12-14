using System;
using System.Threading;

namespace Communications.Transactions
{
    public static class TransactionHelper
    {
        public static TPayload Wait<TPayload>(this ITransaction<TPayload> Transaction)
        {
            return Transaction.Wait(TimeSpan.FromMilliseconds(Timeout.Infinite), new CancellationToken());
        }

        public static TPayload Wait<TPayload>(this ITransaction<TPayload> Transaction, TimeSpan Timeout)
        {
            return Transaction.Wait(Timeout, new CancellationToken());
        }

        public static TPayload Wait<TPayload>(this ITransaction<TPayload> Transaction, CancellationToken CancellationToken)
        {
            return Transaction.Wait(TimeSpan.FromMilliseconds(Timeout.Infinite), CancellationToken);
        }
    }
}