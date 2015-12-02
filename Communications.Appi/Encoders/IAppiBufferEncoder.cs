using System;

namespace Communications.Appi.Encoders
{
    public interface IAppiBufferEncoder<in TMessage>
    {
        Byte[] Encode(TMessage Message);
    }
}
