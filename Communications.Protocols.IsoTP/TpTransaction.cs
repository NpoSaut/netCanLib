using System;
using System.IO;
using System.Threading;

namespace Communications.Protocols.IsoTP
{
    public enum TpTransactionStatus
    {
        Ready,
        Active,
        Done,
        Error
    }

    /// <summary>Представляет абстракцию ISO-TP-транзакции</summary>
    public abstract class TpTransaction
    {
        public int Length
        {
            get { return (int)DataStream.Length; }
        }
        public int Position
        {
            get { return (int)DataStream.Position; }
        }

        public bool Done
        {
            get { return Position == Length; }
        }

        private TpTransactionStatus _status;

        protected TpTransaction(byte[] Data)
        {
            this.Data = Data;
            DataStream = new MemoryStream(Data);
        }

        public Byte[] Data { get; private set; }
        public MemoryStream DataStream { get; private set; }

        /// <summary>Статус транзакции</summary>
        public TpTransactionStatus Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    if (StatusChanged != null) StatusChanged(this, new EventArgs());
                }
            }
        }

        public event EventHandler StatusChanged;

        public void Wait()
        {
            //if (Status == TpTransactionStatus.Ready)
            //    throw new ApplicationException("Транзакция ещё не запущена");

            SpinWait.SpinUntil(() => Status == TpTransactionStatus.Done || Status == TpTransactionStatus.Error);
        }
    }
}
