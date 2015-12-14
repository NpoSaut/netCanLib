using System;
using System.Threading;

namespace Communications.Transactions
{
    public class ExceptionTransaction<TPayload> : ITransaction<TPayload>
    {
        private readonly Exception _exception;
        public ExceptionTransaction(Exception Exception) { _exception = Exception; }

        public TPayload Payload
        {
            get { throw _exception; }
        }

        public bool Done
        {
            get { return true; }
        }

        public TPayload Wait(TimeSpan Timeout, CancellationToken CancellationToken) { throw _exception; }
    }
}
