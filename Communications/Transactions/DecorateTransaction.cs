namespace Communications.Transactions
{
    public class DecorateTransaction<TUpper, TLower> : ITransaction<TUpper>
    {
        private readonly ITransaction<TLower> _coreTransaction;

        public DecorateTransaction(TUpper Payload, ITransaction<TLower> CoreTransaction)
        {
            _coreTransaction = CoreTransaction;
            this.Payload = Payload;
        }

        public TUpper Payload { get; private set; }

        public bool Done
        {
            get { return _coreTransaction.Done; }
        }

        public TUpper Wait()
        {
            _coreTransaction.Wait();
            return Payload;
        }
    }

    public static class DecorateTransactionHelper
    {
        public static ITransaction<TUpper> AsCoreFor<TLower, TUpper>(this ITransaction<TLower> Transaction, TUpper Frame)
        {
            return new DecorateTransaction<TUpper, TLower>(Frame, Transaction);
        }
    }
}
