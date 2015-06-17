using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States
{
    public class ConsecutiveSendState : IsoTpStateBase
    {
        public TpSendTransaction Transaction { get; private set; }
        private int Counter { get; set; }

        public ConsecutiveSendState(IIsoTpConnection Connection, TpSendTransaction Transaction)
            : base(Connection)
        {
            this.Transaction = Transaction;
        }

        public override void Operate(TimeSpan Timeout)
        {
            var frame = new ConsecutiveFrame(Transaction.GetBytes(ConsecutiveFrame.GetPayload(Connection.SubframeLength)), Transaction.Index++);
            Connection.SendFrame(frame);

            Counter++;
            if (Counter == Transaction.BlockSize)
                Connection.SetNextState(new WaitForControlFrameState(Connection, Transaction));

            if (Transaction.Done)
                Connection.OnTransactionReady(Transaction);
        }
    }
}