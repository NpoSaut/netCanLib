using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States
{
    public interface IIsoTpState
    {
        IIsoTpState Operate(IsoTpFrame Frame);
        void OnException(Exception e);
    }

    /// <summary>Базовый класс для состояния ISO-TP транзакции</summary>
    public abstract class IsoTpStateBase : IIsoTpState
    {
        public abstract IIsoTpState Operate(IsoTpFrame Frame);
        public virtual void OnException(Exception e) { }
    }
}
