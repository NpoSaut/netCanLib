using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Appi.Exceptions;
using Communications.Can;
using Buffer = Communications.Appi.Buffers.Buffer;

namespace Communications.Appi
{
    public enum AppiLine : byte
    { 
        Can1 = 0x02,
        Can2 = 0x03
    }

    public delegate void AppiReceiveEventHandler(object sender, AppiMessageRecieveEventArgs e);
    public class AppiMessageRecieveEventArgs : EventArgs
    {
        public IDictionary<AppiLine, IList<CanFrame>> Messages { get; set; }

        public AppiMessageRecieveEventArgs(IDictionary<AppiLine, IList<CanFrame>> Messages)
        {
            this.Messages = Messages;
        }
    }

    internal class AppiBufferReadEventArgs : EventArgs
    {
        public Buffer Buffer { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.EventArgs"/>.
        /// </summary>
        public AppiBufferReadEventArgs(Buffer Buffer) { this.Buffer = Buffer; }
    }

    internal static class AppiCanFrameConstructor
    {
        /// <summary>
        /// Собирает байты для буфера АППИ
        /// </summary>
        /// <returns>10 байт: 2 байта дескриптор + 8 байт данных</returns>
        public static Byte[] ToBufferBytes(this CanFrame Frame)
        {
            Byte[] buff = new Byte[10];

            BitConverter.GetBytes((UInt16)Frame.Descriptor).Reverse().ToArray().CopyTo(buff, 0);
            Frame.Data.CopyTo(buff, 2);
            
            return buff;
        }

        /// <summary>
        /// Восстанавливает CAN-сообщение из буфера АППИ
        /// </summary>
        /// <param name="Buff">10 байт буфера</param>
        public static CanFrame FromBufferBytes(Byte[] Buff)
        {
            int id = (int)BitConverter.ToUInt16(Buff.Take(2).Reverse().ToArray(), 0) >> 4;
            int len = Buff[1] & 0x0f;
            if (len > 8) throw new AppiBufferDecodeException("Расшифрована неправильная длина CAN-сообщения ({0} байт)", len);
            return CanFrame.NewWithId(id, Buff, 2, len);
        }
    }
}
