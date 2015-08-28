using System;
using Communications.Protocols.IsoTP.Frames;
using Communications.Transactions;

namespace Communications.Protocols.IsoTP.StateManagers
{
    public interface ISender
    {
        void Send(IsoTpFrame Frame);
    }

    internal class ActionSender : ISender
    {
        private readonly Func<IsoTpFrame, ITransaction<IsoTpFrame>> _sendAction;
        public ActionSender(Func<IsoTpFrame, ITransaction<IsoTpFrame>> SendAction) { _sendAction = SendAction; }

        public void Send(IsoTpFrame Frame) { _sendAction(Frame); }
    }
}
