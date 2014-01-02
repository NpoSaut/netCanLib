using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States
{
    abstract class IsoTpState
    {
        public ITransactionContext Context { get; private set; }
        public IsoTpState(ITransactionContext Context) { this.Context = Context; }

        public abstract void ProcessFrame(IsoTpFrame Frame);
        
        public event EventHandler<ChangeStateRequestEventArgs> StateChangetRequested;
        protected virtual void OnStateChangetRequested(ChangeStateRequestEventArgs E)
        {
            var handler = StateChangetRequested;
            if (handler != null) handler(this, E);
        }

        public event EventHandler<ExceptionHandledEventArgs> ExceptionHandeled;
        protected virtual void OnExceptionHandeled(HandledEventArgs E)
        {
            var handler = ExceptionHandeled;
            if (handler != null) handler(this, E);
        }
    }

    class ChangeStateRequestEventArgs : EventArgs
    {
        public IsoTpState NextState { get; private set; }
        public ChangeStateRequestEventArgs(IsoTpState NextState) { this.NextState = NextState; }
    }

    class ExceptionHandledEventArgs : EventArgs
    {
        public Exception HandeledException { get; private set; }
        public ExceptionHandledEventArgs(Exception HandeledException) { this.HandeledException = HandeledException; }
    }

    /// <summary>
    /// Состояние отправки последовательных фреймов
    /// </summary>
    class ConsecutiveReceiveState : IsoTpState
    {
        public ConsecutiveReceiveState(ITransactionContext Context) : base(Context) { }

        private int BlocksCounter { get; set; }

        public override void ProcessFrame(IsoTpFrame Frame)
        {
            var cFrame = Frame as ConsecutiveFrame;
            if (cFrame == null) throw new Exception();
            Context.DataStream.Write(cFrame.Data, 0, cFrame.Data.Length);
            BlocksCounter++;
            if (BlocksCounter >= Context.BlockSize) SendControlFrame();
        }
          
        private void SendControlFrame()
        {
            var flowControlFrame = new FlowControlFrame(FlowControlFlag.ClearToSend,
                                                        (byte)Context.BlockSize,
                                                        Context.SeparationTime);
            Context.CanFlow.Send(flowControlFrame.GetCanFrame(Context.SenderDescriptor));
        }
    }
}
