namespace Communications.Transactions
{
    /// <summary>Транзакция</summary>
    public interface ITransaction<out TPayload>
    {
        TPayload Payload { get; }
        bool Done { get; }
        TPayload Wait();
    }
}
