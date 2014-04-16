using System;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;

namespace Communications.Protocols.IsoTP
{
    public abstract class IsoTpConnectionBase : IIsoTpConnection
    {
        private TpTransaction _finishedTransaction;

        protected IsoTpConnectionBase(int ReceiveBlockSize = 128, int SeparationTimeMs = 0)
        {
            this.ReceiveBlockSize = ReceiveBlockSize;
            ReceiveSeparationTime = TimeSpan.FromMilliseconds(SeparationTimeMs);
        }

        public IsoTpState ConnectionState { get; private set; }
        public TimeSpan ReceiveSeparationTime { get; private set; }
        public int ReceiveBlockSize { get; private set; }
        public abstract int SubframeLength { get; }

        public Byte[] Receive(TimeSpan Timeout)
        {
            SetNextState(new ReadyToReceiveState(this));
            do
            {
                ConnectionState.Operate(Timeout);
            } while (_finishedTransaction is TpReceiveTransaction);

            // ReSharper disable once PossibleInvalidCastException
            byte[] res = ((TpReceiveTransaction)_finishedTransaction).Data;
            _finishedTransaction = null;
            return res;
        }

        public void Send(Byte[] Data, TimeSpan Timeout)
        {
            SetNextState(new BeginTransmitionState(this, Data));
            do
            {
                ConnectionState.Operate(Timeout);
            } while (_finishedTransaction is TpReceiveTransaction);
        }

        public void SendControlFrame()
        {
            var flowControlFrame = new FlowControlFrame(FlowControlFlag.ClearToSend, (byte)ReceiveBlockSize, ReceiveSeparationTime);
            SendFrame(flowControlFrame);
        }

        public abstract IsoTpFrame ReadNextFrame(TimeSpan Timeout);
        public abstract void SendFrame(IsoTpFrame Frame);

        public virtual void OnTransactionReady(TpTransaction Transaction) { _finishedTransaction = Transaction; }
        public void SetNextState(IsoTpState NewState) { ConnectionState = NewState; }
    }
}