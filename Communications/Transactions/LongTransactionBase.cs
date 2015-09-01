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
                if (!Done)
                    throw new ApplicationException("Транзакция ещё не была завершена");
                return _payload.Value;
            }
        }

        public abstract bool Done { get; }

        public TPayload Wait()
        {
            _logger.Debug("Ожидаем завершения транзакции {0}", _transactinId);
            _resetEvent.Wait();
            _logger.Debug("Дождались завершения транзакции {0}: {1}", _transactinId, Payload);

            if (_transactionException == null)
                return Payload;
            throw _transactionException;
        }

        protected abstract TPayload GetPayload();

        public void Fail(Exception e)
        {
            _logger.Debug("Фейлим транзакцию {0} -- {1}", _transactinId, e);
            _transactionException = e;
            _resetEvent.Set();
        }

        public void Commit()
        {
            _logger.Debug("Коммитим транзакцию {0}", _transactinId);
            _resetEvent.Set();
        }
    }
}
