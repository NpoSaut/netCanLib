using System;
using Communications.Can;
using Communications.Protocols.IsoTP.Exceptions;

namespace Communications.Protocols.IsoTP.Frames
{
    public enum IsoTpFrameType : byte
    {
        Single = 0x0,
        First = 0x1,
        Consecutive = 0x2,
        FlowControl = 0x3,
        Invalid = 0xff
    }

    /// <summary>Базовый класс для кадров ISO-TP</summary>
    public abstract class IsoTpFrame
    {
        /// <summary>Тип кадра</summary>
        public abstract IsoTpFrameType FrameType { get; }

        /// <summary>Заворачивает кадр ISO-TP в CAN-сообщение</summary>
        /// <param name="WithDescriptor">Дескриптор CAN-сообщения</param>
        public abstract CanFrame GetCanFrame(int WithDescriptor);

        protected abstract void FillWithBytes(Byte[] buff);

        /// <summary>Разбирает буфер сообщения, извлекая оттуда кадр ISO-TP</summary>
        /// <typeparam name="TFrame">Тип кадра ISO-TP</typeparam>
        /// <param name="buff">Буфер данных</param>
        /// <returns>Разобранный кадр</returns>
        public static TFrame ParsePacket<TFrame>(Byte[] buff)
            where TFrame : IsoTpFrame, new()
        {
            var p = new TFrame();
            p.FillWithBytes(buff);
            return p;
        }

        /// <summary>Разбирает буфер сообщения, извлекая оттуда кадр ISO-TP</summary>
        /// <param name="buff">Буфер данных</param>
        /// <returns>Разобранный кадр</returns>
        public static IsoTpFrame ParsePacket(Byte[] buff)
        {
            IsoTpFrame p;
            switch (GetFrameType(buff))
            {
                case IsoTpFrameType.Single:
                    p = new SingleFrame();
                    break;
                case IsoTpFrameType.First:
                    p = new FirstFrame();
                    break;
                case IsoTpFrameType.Consecutive:
                    p = new ConsecutiveFrame();
                    break;
                case IsoTpFrameType.FlowControl:
                    p = new FlowControlFrame();
                    break;
                default:
                    throw new IsoTpProtocolException("Неизвестный тип ISO-TP пакета");
            }
            p.FillWithBytes(buff);
            return p;
        }

        /// <summary>Определяет тип кадра, содержащегося в буфере</summary>
        public static IsoTpFrameType GetFrameType(Byte[] buff)
        {
            int typeCode = (buff[0] & 0xf0) >> 4;
            if (typeCode > 0x3) return IsoTpFrameType.Invalid;
            return (IsoTpFrameType)(typeCode);
        }
    }

    public static class IsoTpFramesHelper
    {
        /// <summary>Возвращает тип пакета ISO-TP</summary>
        public static IsoTpFrameType GetIsoTpFrameType(this CanFrame Frame)
        {
            return IsoTpFrame.GetFrameType(Frame.Data);
        }

        /// <summary>Выполняет парсинг ISO-TP фрейма из CAN-сообщения</summary>
        /// <typeparam name="TFrame">Тип ISO-TP фрейма</typeparam>
        /// <param name="Frame">CAN-сообщение</param>
        public static TFrame ParseAs<TFrame>(this CanFrame Frame)
            where TFrame : IsoTpFrame, new()
        {
            return IsoTpFrame.ParsePacket<TFrame>(Frame.Data);
        }
    }
}
