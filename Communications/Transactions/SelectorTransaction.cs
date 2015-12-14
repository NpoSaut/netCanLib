using System;
using System.Threading;

namespace Communications.Transactions
{
    public class SelectorTransaction<TIn, TOut> : ITransaction<TOut>
    {
        private readonly Func<Exception, Exception> _exceptionSelector;
        private readonly Lazy<TOut> _payload;
        private readonly Func<TIn, TOut> _resultSelector;
        private readonly ITransaction<TIn> _sourceTransaction;

        public SelectorTransaction(ITransaction<TIn> SourceTransaction, Func<TIn, TOut> ResultSelector, Func<Exception, Exception> ExceptionSelector)
        {
            _sourceTransaction = SourceTransaction;
            _resultSelector = ResultSelector;
            _exceptionSelector = ExceptionSelector;
            _payload = new Lazy<TOut>(GetPayload);
        }

        private TOut GetPayload()
        {
            try
            {
                return _resultSelector(_sourceTransaction.Payload);
            }
            catch (Exception e)
            {
                if (_exceptionSelector != null)
                    throw _exceptionSelector(e);
                throw;
            }
        }

        public TOut Payload
        {
            get { return _payload.Value; }
        }

        public bool Done
        {
            get { return _sourceTransaction.Done; }
        }

        public TOut Wait(TimeSpan Timeout, CancellationToken CancellationToken)
        {
            try
            {
                _sourceTransaction.Wait(Timeout, CancellationToken);
                return Payload;
            }
            catch (Exception e)
            {
                if (_exceptionSelector != null)
                    throw _exceptionSelector(e);
                throw;
            }
        }
    }
}
