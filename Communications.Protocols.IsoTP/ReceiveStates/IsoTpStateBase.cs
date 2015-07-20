using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.ReceiveStates
{
    public interface IIsoTpState : IDisposable
    {
        IIsoTpState Operate(IsoTpFrame Frame);
        IIsoTpState OnException(Exception e);
    }

    /// <summary>Базовый класс для состояния ISO-TP транзакции</summary>
    public abstract class IsoTpStateBase : IIsoTpState
    {
        public abstract IIsoTpState Operate(IsoTpFrame Frame);
        public virtual IIsoTpState OnException(Exception e) { return this; }
        public abstract void Dispose();
    }
}
