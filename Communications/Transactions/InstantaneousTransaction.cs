namespace Communications.Transactions
{
    public class InstantaneousTransaction<TPayload> : ITransaction<TPayload>
    {
        public InstantaneousTransaction(TPayload Payload) { this.Payload = Payload; }
        public TPayload Payload { get; private set; }

        public bool Done
        {
            get { return true; }
        }

        public void Wait() { }
    }
}
