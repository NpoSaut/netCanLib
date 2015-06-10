using System;
using Communications.Appi.Buffers;
using Buffer = Communications.Appi.Buffers.Buffer;

namespace Communications.Appi.Decoders
{
    public interface IAppiBufferDecoder
    {
        Buffer DecodeBuffer(Byte[] Buff);
    }
}
