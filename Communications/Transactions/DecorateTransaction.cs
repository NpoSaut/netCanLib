using System;

namespace Communications.Transactions
{
    public class DecorateTransaction<TUpper, TLower> : ITransaction<TUpper>
    {
        private readonly ITransaction<TLower> _coreTransaction;
        private readonly Func<Exception, Exception> _exceptionSelector;

        public DecorateTransaction(TUpper Payload, ITransaction<TLower> CoreTransaction, Func<Exception, Exception> ExceptionSelector)
        {
            _coreTransaction = CoreTransaction;
            _exceptionSelector = ExceptionSelector;
            this.Payload = Payload;
        }

        public TUpper Payload { get; private set; }

        public bool Done
        {
            get { return _coreTransaction.Done; }
        }

        public TUpper Wait()
        {
            try
            {
                _coreTransaction.Wait();
            }
            catch (Exception e)
            {
                if (_exceptionSelector != null)
                    throw _exceptionSelector(e);
                throw;
            }
            return Payload;
        }
    }

    public static class DecorateTransactionHelper
    {
        public static ITransaction<TUpper> AsCoreFor<TLower, TUpper>(this ITransaction<TLower> Transaction, TUpper Frame,
                                                                     Func<Exception, Exception> ExceptionSelector = null)
        {
            return new DecorateTransaction<TUpper, TLower>(Frame, Transaction, ExceptionSelector);
        }
    }
}
