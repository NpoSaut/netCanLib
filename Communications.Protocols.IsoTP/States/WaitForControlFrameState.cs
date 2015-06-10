using System;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States
{
    public class WaitForControlFrameState : IsoTpState
    {
        public WaitForControlFrameState(IIsoTpConnection Connection, TpSendTransaction Transaction)
            : base(Connection) { this.Transaction = Transaction; }

        public TpSendTransaction Transaction { get; private set; }

        private void ProcessFrame(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.FlowControl:
                    var cf = (FlowControlFrame)Frame;
                    switch (cf.Flag)
                    {
                        case FlowControlFlag.ClearToSend:
                            Transaction.BlockSize = cf.BlockSize;
                            Transaction.SeparationTime = cf.SeparationTime;
                            Connection.SetNextState(new ConsecutiveSendState(Connection, Transaction));
                            break;
                        case FlowControlFlag.Wait:
                            break;
                        case FlowControlFlag.Abort:
                            throw new IsoTpTransactionAbortedException();
                    }
                    break;

                case IsoTpFrameType.First:
                case IsoTpFrameType.Single:
                case IsoTpFrameType.Consecutive:
                    throw new IsoTpWrongFrameException(Frame, typeof (ConsecutiveFrame));
            }
        }

        public override void Operate(TimeSpan Timeout) { ProcessFrame(Connection.ReadNextFrame(Timeout)); }
    }
}
