using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States
{
    /// <summary>Состояние готовности к приёму первой транзакции</summary>
    public class ReadyToReceiveState : IsoTpState
    {
        public ReadyToReceiveState(IIsoTpConnection Connection) : base(Connection) { }

        private void ProcessFrame(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.First:
                    var ff = (FirstFrame)Frame;
                    var longTransaction = new TpReceiveTransaction(ff.PacketSize);
                    longTransaction.Write(ff.Data);
                    Connection.SetNextState(new SendControlFrameState(Connection, longTransaction));
                    break;

                case IsoTpFrameType.Single:
                    var sf = (SingleFrame)Frame;
                    var shortTransaction = new TpReceiveTransaction(sf.Data.Length);
                    shortTransaction.Write(sf.Data);
                    Connection.OnTransactionReady(shortTransaction);
                    break;
            }
        }

        public override void Operate(TimeSpan Timeout)
        {
            ProcessFrame(Connection.ReadNextFrame(Timeout));
        }
    }
}