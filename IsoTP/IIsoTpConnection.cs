using System;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;

namespace Communications.Protocols.IsoTP
{
    public interface IIsoTpConnection
    {
        int ReceiveBlockSize { get; }
        TimeSpan ReceiveSeparationTime { get; }
        int SubframeLength { get; }

        void OnTransactionReady(TpTransaction Transaction);
        void SetNextState(IsoTpState NewState);
        void SendControlFrame();

        IsoTpFrame ReadNextFrame(TimeSpan Timeout);
        void SendFrame(IsoTpFrame Frame);

        Byte[] Receive(TimeSpan Timeout);
    }

    public abstract class IsoTpConnectionBase : IIsoTpConnection
    {
        private TpTransaction _readyTransaction;

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
            } while (_readyTransaction is TpReceiveTransaction);

            // ReSharper disable once PossibleInvalidCastException
            byte[] res = ((TpReceiveTransaction)_readyTransaction).Data;
            _readyTransaction = null;
            return res;
        }

        public void Send(Byte[] Data, TimeSpan Timeout)
        {
            SetNextState(new BeginTransmitionState(this, Data));
            do
            {
                ConnectionState.Operate(Timeout);
            } while (_readyTransaction is TpReceiveTransaction);
        }

        public void SendControlFrame()
        {
            var flowControlFrame = new FlowControlFrame(FlowControlFlag.ClearToSend, (byte)ReceiveBlockSize, ReceiveSeparationTime);
            SendFrame(flowControlFrame);
        }

        public abstract IsoTpFrame ReadNextFrame(TimeSpan Timeout);
        public abstract void SendFrame(IsoTpFrame Frame);

        public virtual void OnTransactionReady(TpTransaction Transaction) { _readyTransaction = Transaction; }
        public void SetNextState(IsoTpState NewState) { ConnectionState = NewState; }
    }
}
