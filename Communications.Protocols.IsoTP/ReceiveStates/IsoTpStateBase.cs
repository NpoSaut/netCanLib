using System;
using System.Collections.Generic;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.ReceiveStates
{
    public interface IIsoTpState : IDisposable
    {
        void Activate();
        IIsoTpState Operate(IsoTpFrame Frame);
        IIsoTpState OnException(Exception e);
        void Abort();
        event EventHandler<WannaSendEventArgs> WannaSend;
    }

    public class WannaSendEventArgs : EventArgs
    {
        public WannaSendEventArgs(IList<IsoTpFrame> Frames) { this.Frames = Frames; }
        public IList<IsoTpFrame> Frames { get; private set; }
    }

    /// <summary>Базовый класс для состояния ISO-TP транзакции</summary>
    public abstract class IsoTpStateBase : IIsoTpState
    {
        public abstract IIsoTpState Operate(IsoTpFrame Frame);
        public virtual IIsoTpState OnException(Exception e) { return this; }
        public abstract void Dispose();
    }
}
