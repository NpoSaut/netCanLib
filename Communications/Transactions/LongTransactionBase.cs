using System;
using System.Threading;

namespace Communications.Transactions
{
    public abstract class LongTransactionBase<TPayload> : ITransaction<TPayload>
    {
        private readonly Lazy<TPayload> _payload;
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private Exception _transactionException;

        protected LongTransactionBase() { _payload = new Lazy<TPayload>(GetPayload); }

        public TPayload Payload
        {
            get
            {
                if (!Done)
                    throw new ApplicationException("Транзакция ещё не была завершена");
                return _payload.Value;
            }
        }

        public abstract bool Done { get; }

        public void Wait()
        {
            _resetEvent.WaitOne();
            if (_transactionException != null)
                throw _transactionException;
        }

        protected abstract TPayload GetPayload();

        public void Fail(Exception e)
        {
            _transactionException = e;
            _resetEvent.Set();
        }

        public void Commit() { _resetEvent.Set(); }
    }
}
