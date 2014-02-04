using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Communications.Can;
using SocketCanWorking.Exceptions;

namespace SocketCanWorking
{
    /// <summary>
    /// Оболочка над функциями, экспортируемыми из SocketCan
    /// </summary>
    public unsafe static class SocketCanLib
    {
        /// <summary>
        /// Структура CAN-фрейма в формате SocketCan
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct SocketCanFrame
        {
            /// <remarks>
            /// bit 0-28  : Can identifier (11/29 bit)
            /// bit 29    : error frame flag (0 = data frame, 1 = error frame)
            /// bit 30    : remote transmission request flag (1 = rtr frame)
            /// bit 31    : frame format flag (0 = standart 11 bit, 1 = extended 29 bit)
            /// </remarks>
            public UInt32 Id;
            public Byte DataLength;
            public fixed byte Data[8];

            public SocketCanFrame(uint Id, byte[] Data)
                : this()
            {
                this.Id = Id;
                this.DataLength = (Byte)Data.Length;
                fixed (byte* d = this.Data)
                {
                    for (int i = 0; i < Data.Length; i++)
                    {
                        d[i] = Data[i];
                    }
                }
            }
            public SocketCanFrame(CanFrame Frame) : this((uint)Frame.Id, Frame.Data) { }
        }

        #region Импорт функций из библиотеки

        /// <summary>
        /// Имя библиотеки-связки с SocketCan
        /// </summary>
        private const string SocketCanLibraryName = "libSocketCanLib.so.1";

        /// <summary>
        /// Открывает сокет
        /// </summary>
        /// <param name="InterfaceName">Имя сокета в виде c-строки</param>
        /// <returns>Номер открытого сокета</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SocketOpen(byte[] InterfaceName);

        /// <summary>
        /// Закрывает сокет
        /// </summary>
        /// <param name="Number">Номер сокета</param>
        /// <returns>Антон придумает</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SocketClose(int Number);

        /// <summary>
        /// Отправляет в сокет
        /// </summary>
        /// <param name="Number">Номер сокета</param>
        /// <param name="Frame">Фрейм для отправки</param>
        /// <returns>True, если фрейм успешно отправлен</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SocketWrite(int Number, SocketCanFrame* Frame);

        /// <summary>
        /// Читает из сокета
        /// </summary>
        /// <param name="Number">Номер сокета</param>
        /// <param name="Frame">Фрейм для отправки</param>
        /// <returns>True, если что-то прочитано</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SocketRead(int Number, SocketCanFrame* Frame);

        #endregion

        #region Оборачивание библиотечных функций

        private static Encoder encoder = Encoding.ASCII.GetEncoder();

        private static byte[] GetCString(String str)
        {
            var cString = new byte[str.Length + 1];
            encoder.GetBytes(str.ToCharArray(), 0, str.Length, cString, 0, true);
            cString[cString.Length - 1] = (byte)char.MinValue;
            return cString;
        }

        private static CanFrame GetCanFrame(SocketCanFrame scFrame)
        {
            byte[] data = new byte[scFrame.DataLength];
            for (int i = 0; i < data.Length; i++) data[i] = scFrame.Data[i];
            var res = CanFrame.NewWithId((int)scFrame.Id, data);
            res.Time = DateTime.Now;
            return res;
        }

        /// <summary>
        /// Открывает сокет
        /// </summary>
        /// <param name="InterfaceName">Имя сокета в виде c-строки</param>
        /// <exception cref="SocketCanOpenException">Ошибка при попытке открыть сокет</exception>
        public static int Open(String InterfaceName)
        {
            var number = SocketOpen(GetCString(InterfaceName));
            if (number <= 0) throw new SocketCanOpenException(-number);
            return number;
        }

        /// <summary>
        /// Закрывает сокет
        /// </summary>
        /// <param name="Number">Номер сокета</param>
        public static void Close(int Number) { SocketClose(Number); }

        /// <summary>
        /// Отправляет CAN-фрейм
        /// </summary>
        /// <param name="SocketNumber">Номер сокета для отправки</param>
        /// <param name="Frame">Фрейм для отправки</param>
        public static void Write(int SocketNumber, CanFrame Frame)
        {
            var scFrame = new SocketCanFrame(Frame);
            int writeStatus = SocketWrite(SocketNumber, &scFrame);
            if (writeStatus < 0) throw new SocketCanWriteException(-writeStatus);
        }

        /// <summary>
        /// Пытается прочитать фрейм
        /// </summary>
        /// <param name="SocketNumber">Номер сокета для чтения</param>
        /// <returns>Прочитаный фрейм или null, если нет фреймов в буфере</returns>
        public static CanFrame Read(int SocketNumber)
        {
            var scFrame = new SocketCanFrame();
            int readStatus = SocketRead(SocketNumber, &scFrame);
            switch (readStatus)
            {
                case 0:
                    return null;
                case 1:
                    return GetCanFrame(scFrame);
                default:
                    throw new SocketCanReadException(-readStatus);
            }
        }

        #endregion

    }
}