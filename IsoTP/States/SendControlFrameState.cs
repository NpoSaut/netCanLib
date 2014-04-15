using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States
{
    public class SendControlFrameState : IsoTpState
    {
        public TpReceiveTransaction Transaction { get; set; }

        public SendControlFrameState(IIsoTpConnection Connection, TpReceiveTransaction Transaction) : base(Connection)
        {
            this.Transaction = Transaction;
        }

        public override void Operate(TimeSpan Timeout)
        {
            var frame = new FlowControlFrame(FlowControlFlag.ClearToSend, (byte)Connection.ReceiveBlockSize,
                                             Connection.ReceiveSeparationTime);
            Connection.SendFrame(frame);
            Connection.SetNextState(new SendControlFrameState(Connection, Transaction));
        }
    }
}