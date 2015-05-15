using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Appi.Buffers;
using Communications.Appi.Exceptions;
using Communications.Can;

namespace Communications.Appi.Decoders
{
    public class AppiLineStatusDecoder : IAppiLineStatusDecoder
    {
        private readonly AppiLineStatusLayout _layout;
        public AppiLineStatusDecoder(AppiLineStatusLayout Layout) { _layout = Layout; }

        /// <summary>Декодирует статус Can-линии АППИ</summary>
        /// <param name="Buff">Буфер АППИ</param>
        public AppiLineStatus DecodeLineStatus(byte[] Buff)
        {
            return new AppiLineStatus(BitConverter.ToUInt16(Buff, _layout.BaudRateOffset),
                                      ExtractFrames(Buff),
                                      Buff[_layout.SendQueueOffset],
                                      Buff[_layout.SendErrorIndex],
                                      Buff[_layout.ReceiveErrorIndex]);
        }

        /// <summary>Декодирует поток входящих сообщений из буфера</summary>
        /// <param name="Buff">Буфер с входящими сообщениями</param>
        private IEnumerable<CanFrame> ExtractFrames(byte[] Buff)
        {
            int count = Buff[_layout.FramesCountOffset];
            for (int i = 0; i < count; i++)
            {
                int id = BitConverter.ToUInt16(Buff.Take(2).Reverse().ToArray(), 0) >> 4;
                int len = Buff[1 + _layout.FramesBodyOffset] & 0x0f;
                if (len > 8)
                    throw new AppiBufferDecodeException("Расшифрована неправильная длина CAN-сообщения ({0} байт)", len);
                yield return CanFrame.NewWithId(id, Buff, 2 + _layout.FramesBodyOffset, len);
            }
        }
    }
}
