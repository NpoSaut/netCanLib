using System;
using Communications.Protocols.IsoTP.Contexts;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States.Receive;

namespace Communications.Protocols.IsoTP.States
{
//    public class SendControlFrameState : IsoTpStateBase
//    {
//        private readonly IsoTpReceiveTransactionContext _transactionContext;
//        public SendControlFrameState(IsoTpReceiveTransactionContext TransactionContext) { _transactionContext = TransactionContext; }
//
//        public override void Operate(TimeSpan Timeout)
//        {
//            var frame = new FlowControlFrame(FlowControlFlag.ClearToSend, (byte)Connection.ReceiveBlockSize,
//                                             Connection.ReceiveSeparationTime);
//            Connection.SendFrame(frame);
//            Connection.SetNextState(new ConsecutiveReceiveState(Connection, Transaction));
//        }
//
//        public override IIsoTpState Operate(IsoTpFrame Frame)
//        {
//            
//        }
//    }
}