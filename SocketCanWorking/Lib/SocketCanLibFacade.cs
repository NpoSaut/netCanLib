using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using SocketCanWorking.Exceptions;

namespace SocketCanWorking.Lib
{
    /// <summary>Используется для того, чтобы можно было уйти от статических методов, использованных в библиотеке SocketCanLib</summary>
    public unsafe class SocketCanLibFacade : ISocketCanLibFacade
    {
        public const int ReceiveBufferLength = 16;
        private static readonly Encoder Encoder = Encoding.ASCII.GetEncoder();
        public SocketCanLibFacade() { }

        /// <summary>Открывает сокет.</summary>
        /// <param name="InterfaceName">Имя сокета в виде c-строки.</param>
        /// <exception cref="SocketCanOpenException">Ошибка при попытке открыть сокет.</exception>
        public int Open(String InterfaceName)
        {
            int number = SocketCanLib.SocketOpen(GetCString(InterfaceName));
            if (number <= 0) throw new SocketCanOpenException(-number);
            return number;
        }

        /// <summary>Закрывает сокет.</summary>
        /// <param name="Number">Номер сокета.</param>
        public void Close(int Number) { SocketCanLib.SocketClose(Number); }

        /// <summary>Отправляет CAN-фрейм.</summary>
        /// <param name="SocketNumber">Номер сокета для отправки.</param>
        /// <param name="Frame">Фрейм для отправки.</param>
        public void Write(int SocketNumber, IList<CanFrame> Frame)
        {
            foreach (var frame in Frame)
            {
                var scFrame = new SocketCanFdFrame(frame);
                int writeStatus = SocketCanLib.SocketWrite(SocketNumber, &scFrame);
                if (writeStatus < 0) throw new SocketCanWriteException(-writeStatus);
            }
        }

        /// <summary>Пытается прочитать фреймы из сокета.</summary>
        /// <param name="SocketNumber">Номер сокета для чтения.</param>
        /// <param name="Timeout">Таймаут ожидания получения сообщения в случае, если во входящем буфере не оказалось сообщений.</param>
        /// <returns>Список фреймов, прочитанных из указанного сокета.</returns>
        public IList<CanFrame> Read(int SocketNumber, TimeSpan Timeout)
        {
            var bags = new FrameBag[ReceiveBufferLength];
            int result;
            fixed (FrameBag* bagsPtr = bags)
            {
                result = SocketCanLib.SocketRead(SocketNumber, bagsPtr, ReceiveBufferLength, (int)Timeout.TotalMilliseconds);
            }

            foreach (FrameBag bag in bags.Take(result))
                Console.WriteLine("    -> BAG: {{ {0} }}", bag);

            if (result >= 0) return bags.Take(result).Select(GetCanFrame).ToList();
            throw new SocketCanReadException(-result);
        }

        private static byte[] GetCString(String str)
        {
            var cString = new byte[str.Length + 1];
            Encoder.GetBytes(str.ToCharArray(), 0, str.Length, cString, 0, true);
            cString[cString.Length - 1] = (byte)char.MinValue;
            return cString;
        }

        private static CanFrame GetCanFrame(FrameBag Bag)
        {
            SocketCanFdFrame scFrame = Bag.Frame;

            var data = new byte[scFrame.DataLength];
            for (int i = 0; i < data.Length; i++) data[i] = scFrame.Data[i];
            CanFrame res = CanFrame.NewWithId((int)scFrame.Id, data);
            res.Time = Bag.ReceiveTime;
            res.IsLoopback = Bag.Flags.HasFlag(FrameBagFlags.Loopback);
            return res;
        }
    }
}
