using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    internal interface IIsoTpFramesPort : IPort<IsoTpFrame, IsoTpFramesPortOptions> { }
}