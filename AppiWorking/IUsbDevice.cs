using System;

namespace Communications.Appi
{
    public interface IUsbDevice
    {
        Byte[] ReadBuffer();
        void WriteBuffer(Byte[] Buffer);
    }
}
