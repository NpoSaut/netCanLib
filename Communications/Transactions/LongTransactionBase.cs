using System;
using System.Threading;
using NLog;

namespace Communications.Transactions
{
    public abstract class LongTransactionBase<TPayload> : ITransaction<TPayload>
    {
        private static int x;
        private readonly ILogger _logger = LogManager.GetLogger("Transactions");
        private readonly Lazy<TPayload> _payload;
        private readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim(false);
        private readonly int _transactinId;
        private Exception _transactionException;

        protected LongTransactionBase()
        {
            _payload = new Lazy<TPayload>(GetPayload);
            _transactinId = Interlocked.Increment(ref x);
            _logger.Debug("Создали транзакцию {0}", _transactinId);
        }

        public TPayload Payload
        {
            get
            {
                if (!_resetEvent.IsSet)
                    throw new ApplicationException("Транзакция ещё не была завершена");
                if (_transactionException != null)
                    throw _transactionException;
                return _payload.Value;
            }
        }

        public bool Done
        {
            get { return _resetEvent.IsSet; }
        }

        public TPayload Wait(TimeSpan Timeout, CancellationToken CancellationToken)
        {
            _logger.Debug("Ожидаем завершения транзакции {0}", _transactinId);
            if (!_resetEvent.Wait(Timeout, CancellationToken))
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();
                throw new TimeoutException();
            }
            _logger.Debug("Дождались завершения транзакции {0}: {1}", _transactinId, Payload);
            return Payload;
        }

        protected abstract TPayload GetPayload();

        public void Fail(Exception e)
        {
            _logger.Debug("Фейлим транзакцию {0} -- {1}", _transactinId, e);
            _transactionException = e;
            _resetEvent.Set();
            OnFailed();
            OnCompleated();
        }

        public void Commit()
        {
            _logger.Debug("Коммитим транзакцию {0}", _transactinId);
            _resetEvent.Set();
            OnCommited();
            OnCompleated();
        }

        protected virtual void OnCommited() { }
        protected virtual void OnFailed() { }
        protected virtual void OnCompleated() { }
    }
}
