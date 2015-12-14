using System;
using System.Reactive.Linq;
using System.Threading;
using Communications.Transactions;

namespace Communications.PortHelpers
{
    public static class PortTransactionHelper
    {
        public static IObservable<TFrame> WaitForTransactionCompleated<TFrame>(this IObservable<ITransaction<TFrame>> Source, TimeSpan Timeout,
                                                                               CancellationToken CancellationToken)
        {
            return Source.Do(transaction => transaction.Wait(Timeout, CancellationToken))
                         .Select(transaction => transaction.Payload);
        }

        public static IObservable<TFrame> WaitForTransactionCompleated<TFrame>(this IObservable<ITransaction<TFrame>> Source,
                                                                               CancellationToken CancellationToken)
        {
            return WaitForTransactionCompleated(Source, TimeSpan.FromMilliseconds(Timeout.Infinite), CancellationToken);
        }

        public static IObservable<TFrame> WaitForTransactionCompleated<TFrame>(this IObservable<ITransaction<TFrame>> Source, TimeSpan Timeout)
        {
            return WaitForTransactionCompleated(Source, Timeout, new CancellationToken());
        }

        public static IObservable<TFrame> WaitForTransactionCompleated<TFrame>(this IObservable<ITransaction<TFrame>> Source)
        {
            return WaitForTransactionCompleated(Source, TimeSpan.FromMilliseconds(Timeout.Infinite), new CancellationToken());
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
