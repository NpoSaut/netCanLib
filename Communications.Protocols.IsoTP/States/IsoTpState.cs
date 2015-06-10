using System;

namespace Communications.Protocols.IsoTP.States
{
    /// <summary>Базовый класс для состояния ISO-TP транзакции</summary>
    public abstract class IsoTpState
    {
        protected IsoTpState(IIsoTpConnection Connection) { this.Connection = Connection; }
        protected IIsoTpConnection Connection { get; private set; }

        public abstract void Operate(TimeSpan Timeout);
        public virtual void OnException(Exception e) { }
    }
}
