using System;
using System.IO;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States
{
    /// <summary>Базовый класс для состояния ISO-TP транзакции</summary>
    public abstract class IsoTpState
    {
        protected IsoTpState(IIsoTpConnection Connection) { this.Connection = Connection; }
        protected IIsoTpConnection Connection { get; private set; }

        public abstract void ProcessFrame(IsoTpFrame Frame);
    }

    /// <summary>Состояние готовности к приёму первой транзакции</summary>
    public class ReadyToReceiveState : IsoTpState
    {
        public ReadyToReceiveState(IIsoTpConnection Connection) : base(Connection) { }

        public override void ProcessFrame(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.First:
                    var ff = (FirstFrame)Frame;
                    var longTransaction = new TpReceiveTransaction(ff.PacketSize);
                    longTransaction.Write(ff.Data);
                    Connection.SendControlFrame();
                    Connection.SetNextState(new ConsecutiveReceiveState(Connection, longTransaction));
                    break;

                case IsoTpFrameType.Single:
                    var sf = (SingleFrame)Frame;
                    var shortTransaction = new TpReceiveTransaction(sf.Data.Length);
                    shortTransaction.Write(sf.Data);
                    Connection.OnTransactionReady(shortTransaction);
                    break;
            }
        }
    }

    /// <summary>Состояние отправки последовательных фреймов</summary>
    public class ConsecutiveReceiveState : IsoTpState
    {
        public ConsecutiveReceiveState(IIsoTpConnection Connection, TpReceiveTransaction Transaction) : base(Connection)
        {
            this.Transaction = Transaction;
            BlockCounter = 0;
        }

        public TpReceiveTransaction Transaction { get; set; }
        private int BlockCounter { get; set; }

        public override void ProcessFrame(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.Consecutive:
                    var cf = (ConsecutiveFrame)Frame;

                    if (cf.Index != Transaction.ExpectedFrameIndex) throw new IsoTpSequenceException(Transaction.ExpectedFrameIndex, cf.Index);
                    Transaction.ExpectedFrameIndex++;

                    Transaction.Write(cf.Data);
                    BlockCounter++;

                    if (BlockCounter == Connection.BlockSize) Connection.SendControlFrame();
                    if (Transaction.Done) Connection.OnTransactionReady(Transaction);
                    break;

                case IsoTpFrameType.First:
                case IsoTpFrameType.FlowControl:
                case IsoTpFrameType.Single:
                    throw new IsoTpProtocolException(
                        String.Format("Было получено сообщение типа {0} в то время, как ожидалось {1}",
                                      Frame.GetType().Name, typeof (ConsecutiveFrame).Name));
            }
        }
    }
}
