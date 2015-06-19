using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States.Send
{
    public class IdleSendState : IsoTpStateBase
    {
        public override IIsoTpState Operate(IsoTpFrame Frame) { return this; }
    }
}
