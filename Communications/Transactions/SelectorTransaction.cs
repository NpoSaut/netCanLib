using System;

namespace Communications.Transactions
{
    public struct SelectorTransaction<TIn, TOut> : ITransaction<TOut>
    {
        private readonly Lazy<TOut> _payload;
        private readonly Func<TIn, TOut> _resultSelector;
        private readonly ITransaction<TIn> _sourceTransaction;

        public SelectorTransaction(ITransaction<TIn> SourceTransaction, Func<TIn, TOut> ResultSelector)
        {
            _sourceTransaction = SourceTransaction;
            _resultSelector = ResultSelector;
            _payload = new Lazy<TOut>(() => ResultSelector(SourceTransaction.Payload));
        }

        public TOut Payload
        {
            get { return _payload.Value; }
        }

        public bool Done
        {
            get { return _sourceTransaction.Done; }
        }

        public TOut Wait()
        {
            _sourceTransaction.Wait();
            return Payload;
        }
    }
}
