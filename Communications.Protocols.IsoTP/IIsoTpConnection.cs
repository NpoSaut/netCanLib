using Communications.Protocols.IsoTP.Transactions;

namespace Communications.Protocols.IsoTP
{
    public interface IIsoTpConnection : IDataPort<IsoTpPacket> { }
}
