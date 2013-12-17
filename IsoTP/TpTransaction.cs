using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using Communications.Protocols.IsoTP.Exceptions;

namespace Communications.Protocols.IsoTP
{
    public enum TpTransactionStatus { Ready, Active, Done, Error }
    /// <summary>
    /// Представляет абстракцию ISO-TP-транзакции
    /// </summary>
    public abstract class TpTransaction
    {
        /// <summary>
        /// Используемый порт
        /// </summary>
        public ICanSocket Socket { get; private set; }
        /// <summary>
        /// Используемый дескриптор
        /// </summary>
        public int TransmitDescriptor { get; private set; }
        /// <summary>
        /// Используемый дескриптор
        /// </summary>
        public int AcknowledgmentDescriptor { get; private set; }
        /// <summary>
        /// Время ожидания пакета
        /// </summary>
        public TimeSpan Timeout { get; set; }
        public static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(3);

        private TpTransactionStatus _Status;
        public event EventHandler StatusChanged;
        /// <summary>
        /// Статус транзакции
        /// </summary>
        public TpTransactionStatus Status
        {
            get { return _Status; }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    if (StatusChanged != null) StatusChanged(this, new EventArgs());
                }
            }
        }

        public TpTransaction(ICanSocket Socket, int TransmitDescriptor, int AcknowledgmentDescriptor)
        {
            this.TransmitDescriptor = TransmitDescriptor;
            this.AcknowledgmentDescriptor = AcknowledgmentDescriptor;
            this.Socket = Socket;
            this.Timeout = DefaultTimeout;
            this.Status = TpTransactionStatus.Ready;
        }

        public void Wait()
        {
            //if (Status == TpTransactionStatus.Ready)
            //    throw new ApplicationException("Транзакция ещё не запущена");

            System.Threading.SpinWait.SpinUntil(() => Status == TpTransactionStatus.Done || Status == TpTransactionStatus.Error);
        }
    }
}
