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

        public static IObservable<ITransaction<TOut>> SelectTransaction<TIn, TOut>(this IObservable<ITransaction<TIn>> Source,
                                                                                   Func<TIn, TOut> ResultSelector)
        {
            return SelectTransaction(Source, ResultSelector, null);
        }

        public static IObservable<ITransaction<TOut>> SelectTransaction<TIn, TOut>(this IObservable<ITransaction<TIn>> Source,
                                                                                   Func<TIn, TOut> ResultSelector, Func<Exception, Exception> ExceptionSelector)
        {
            return Source.Select(transaction => (ITransaction<TOut>)new SelectorTransaction<TIn, TOut>(transaction, ResultSelector, ExceptionSelector));
        }

        public static IObservable<ITransaction<TOut>> SelectTransaction<TIn, TOut>(this IObservable<TIn> Source,
                                                                                   Func<TIn, TOut> ResultSelector, Func<Exception, Exception> ExceptionSelector)
        {
            return Source.Select(arg => 
                                 {
                                     try
                                     {
                                         return (ITransaction<TOut>)new InstantaneousTransaction<TOut>(ResultSelector(arg));
                                     }
                                     catch (Exception e)
                                     {
                                         return (ITransaction<TOut>)new ExceptionTransaction<TOut>(ExceptionSelector(e));
                                     }
                                 });
        }
    }
}
