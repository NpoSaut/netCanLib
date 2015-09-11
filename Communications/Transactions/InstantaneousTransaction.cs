using System;

namespace Communications.Transactions
{
    public class InstantaneousTransaction<TPayload> : ITransaction<TPayload>
    {
        private readonly Exception _exception;
        private readonly TPayload _payload;

        public InstantaneousTransaction(Func<TPayload> PayloadFactory)
        {
            try
            {
                _payload = PayloadFactory();
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        public InstantaneousTransaction(TPayload Payload) { _payload = Payload; }

        public TPayload Payload
        {
            get
            {
                if (_exception != null)
                    throw _exception;
                return _payload;
            }
        }

        public bool Done
        {
            get { return true; }
        }

        public TPayload Wait() { return Payload; }
    }
}
