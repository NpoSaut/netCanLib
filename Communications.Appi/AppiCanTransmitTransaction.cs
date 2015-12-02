using Communications.Can;
using Communications.Transactions;

namespace Communications.Appi
{
    public class AppiCanTransmitTransaction : LongTransactionBase<CanFrame>
    {
        private readonly CanFrame _frame;
        public AppiCanTransmitTransaction(CanFrame Frame) { _frame = Frame; }
        protected override CanFrame GetPayload() { return _frame; }
    }
}
