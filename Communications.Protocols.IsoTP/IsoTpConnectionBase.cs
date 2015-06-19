using System;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;
using Communications.Protocols.IsoTP.States.Receive;

namespace Communications.Protocols.IsoTP
{
//    public abstract class IsoTpConnectionBase : IIsoTpConnection
//    {
//        private TpTransaction _finishedTransaction;
//
//        protected IsoTpConnectionBase(int ReceiveBlockSize = 128, int SeparationTimeMs = 0)
//        {
//            this.ReceiveBlockSize = ReceiveBlockSize;
//            ReceiveSeparationTime = TimeSpan.FromMilliseconds(SeparationTimeMs);
//        }
//
//        public IsoTpStateBase ConnectionState { get; private set; }
//        public TimeSpan ReceiveSeparationTime { get; private set; }
//        public int ReceiveBlockSize { get; private set; }
//
//        public int MaximumDatagramLength
//        {
//            get { return 4095; }
//        }
//
//        public abstract int SubframeLength { get; }
//
//        public Byte[] Receive(TimeSpan Timeout)
//        {
//            SetNextState(new ReadyToReceiveState(this));
//            var transaction = OperateUntilTransactionFinished<TpReceiveTransaction>(Timeout);
//            return transaction.Data;
//        }
//
//        public abstract IsoTpFrame ReadNextFrame(TimeSpan Timeout);
//        public abstract void SendFrame(IsoTpFrame Frame);
//
//        public virtual void OnTransactionReady(TpTransaction Transaction) { _finishedTransaction = Transaction; }
//        public void SetNextState(IsoTpStateBase NewState) { ConnectionState = NewState; }
//
//        private void Operate(TimeSpan Timeout)
//        {
//            try
//            {
//                ConnectionState.Operate();
//            }
//            catch (Exception e)
//            {
//                ConnectionState.OnException(e);
//                throw;
//            }
//        }
//
//        private TTransaction OperateUntilTransactionFinished<TTransaction>(TimeSpan Timeout)
//            where TTransaction : TpTransaction
//        {
//            do
//            {
//                Operate(Timeout);
//            } while (_finishedTransaction == null);
//            
//            if (!(_finishedTransaction is TTransaction))
//                throw new IsoTpProtocolException("Операция завершилась транзакцией неверного типа");
//            
//            var transaction = (TTransaction)_finishedTransaction;
//            _finishedTransaction = null;
//            return transaction;
//        }
//
//        public void Send(Byte[] Data, TimeSpan Timeout)
//        {
//            SetNextState(new BeginTransmitionState(this, Data));
//            OperateUntilTransactionFinished<TpSendTransaction>(Timeout);
//        }
//    }
}
