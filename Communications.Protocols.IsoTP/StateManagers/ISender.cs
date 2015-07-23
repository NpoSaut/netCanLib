using System;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.StateManagers
{
    public interface ISender
    {
        void Send(IsoTpFrame Frame);
    }

    internal class ActionSender : ISender
    {
        private readonly Action<IsoTpFrame> _sendAction;
        public ActionSender(Action<IsoTpFrame> SendAction) { _sendAction = SendAction; }

        public void Send(IsoTpFrame Frame) { _sendAction(Frame); }
    }
}
