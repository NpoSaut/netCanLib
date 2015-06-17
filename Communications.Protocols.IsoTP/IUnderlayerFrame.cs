using System;

namespace Communications.Protocols.IsoTP
{
    public interface IUnderlayerFrame
    {
        Byte[] Data { get; } 
    }
}