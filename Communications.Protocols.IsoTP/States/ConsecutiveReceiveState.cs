using System;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States
{
    /// <summary>Состояние отправки последовательных фреймов</summary>
    public class ConsecutiveReceiveState : IsoTpState
    {
        public ConsecutiveReceiveState(IIsoTpConnection Connection, TpReceiveTransaction Transaction) : base(Connection)
        {
            this.Transaction = Transaction;
            Counter = 0;
        }

        public TpReceiveTransaction Transaction { get; set; }
        private int Counter { get; set; }

        private void ProcessFrame(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.Consecutive:
                    var cf = (ConsecutiveFrame)Frame;

                    if (cf.Index != Transaction.ExpectedFrameIndex)
                        throw new IsoTpSequenceException(Transaction.ExpectedFrameIndex, cf.Index);

                    Transaction.ExpectedFrameIndex = (byte)((Transaction.ExpectedFrameIndex + 1) & 0x0f);
                    Transaction.Write(cf.Data);
                    Counter++;

                    if (Counter == Connection.ReceiveBlockSize) Connection.SetNextState(new SendControlFrameState(Connection, Transaction));
                    if (Transaction.Done) Connection.OnTransactionReady(Transaction);
                    break;

                case IsoTpFrameType.First:
                case IsoTpFrameType.FlowControl:
                case IsoTpFrameType.Single:
                    throw new IsoTpWrongFrameException(Frame, typeof (ConsecutiveFrame));
            }
        }

        public override void Operate(TimeSpan Timeout)
        {
            ProcessFrame(Connection.ReadNextFrame(Timeout));
        }

        public override void OnException(Exception e)
        {
            Connection.SendFrame(FlowControlFrame.AbortFrame);
            base.OnException(e);
        }
    }
}