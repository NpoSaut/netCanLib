using System;
using Communications.Appi.Buffers;

namespace Communications.Appi.Decoders
{
    /// <summary>Инструмент по декодированию сообщений из входящего буфера АППИ</summary>
    public interface IAppiLineStatusDecoder
    {
        /// <summary>Декодирует статус Can-линии АППИ</summary>
        /// <param name="Buff">Буфер АППИ</param>
        AppiLineStatus DecodeLineStatus(Byte[] Buff);
    }
}
