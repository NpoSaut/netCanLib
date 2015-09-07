using System;

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

        public TPayload Wait() { throw _exception; }
    }
}
