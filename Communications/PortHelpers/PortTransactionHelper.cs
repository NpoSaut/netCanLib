using System;
using System.Reactive.Linq;
using Communications.Transactions;

namespace Communications.PortHelpers
{
    public static class PortTransactionHelper
    {
        public static IObservable<TFrame> WaitForTransactionCompleated<TFrame>(this IObservable<ITransaction<TFrame>> Source)
        {
            return Source.Do(transaction => transaction.Wait())
                         .Select(transaction => transaction.Payload);
        }

        public static IObservable<ITransaction<TOut>> SelectTransaction<TIn, TOut>(this IObservable<ITransaction<TIn>> Source, Func<TIn, TOut> ResultSelector)
        {
            return Source.Select(transaction => (ITransaction<TOut>)new SelectorTransaction<TIn, TOut>(transaction, ResultSelector));
        }
    }
}
