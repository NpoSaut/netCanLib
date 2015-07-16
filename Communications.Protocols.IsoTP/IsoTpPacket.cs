using System;

namespace Communications.Protocols.IsoTP
{
    public class IsoTpPacket : IDataFrame
    {
        public IsoTpPacket(byte[] Data) { this.Data = Data; }
        public Byte[] Data { get; private set; } 
    }
}