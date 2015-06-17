using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States
{
    public class BeginTransmitionState : IsoTpStateBase
    {
        public byte[] SendData { get; private set; }

        public BeginTransmitionState(IIsoTpConnection Connection, byte[] SendData) : base(Connection)
        {
            this.SendData = SendData;
        }

        public override void Operate(TimeSpan Timeout)
        {
            var transaction = new TpSendTransaction(SendData);
            if (SendData.Length <= SingleFrame.GetPayload(Connection.SubframeLength))
            {
                var frame = new SingleFrame(SendData);
                Connection.SendFrame(frame);
                Connection.OnTransactionReady(transaction);
            }
            else
            {
                var frame = new FirstFrame(transaction.GetBytes(FirstFrame.GetPayload(Connection.SubframeLength)), transaction.Length);
                Connection.SendFrame(frame);
                Connection.SetNextState(new WaitForControlFrameState(Connection, transaction));
            }
        }
    }
}