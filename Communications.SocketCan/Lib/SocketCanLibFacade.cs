using System;
using System.Text;
using Communications.Can;
using Communications.SocketCan.Exceptions;

namespace Communications.SocketCan.Lib
{
    /// <summary>Используется для того, чтобы можно было уйти от статических методов, использованных в библиотеке SocketCanLib</summary>
    public unsafe class SocketCanLibFacade : ISocketCanLibFacade
    {
        private static readonly Encoder _encoder = Encoding.ASCII.GetEncoder();

        /// <summary>Открывает сокет.</summary>
        /// <param name="InterfaceName">Имя сокета</param>
        /// <param name="RxBuffSize">Размер буфера входящих сообщений</param>
        /// <param name="TxBuffSize">Размер буфера исходящих сообщений</param>
        /// <exception cref="SocketCanOpenException">Ошибка при попытке открыть сокет.</exception>
        public int Open(string InterfaceName, int RxBuffSize, int TxBuffSize)
        {
            int number = SocketCanLib.SocketOpen(GetCString(InterfaceName), TxBuffSize, RxBuffSize);
            if (number <= 0) throw new SocketCanOpenException(-number);
            return number;
        }

        /// <summary>Закрывает сокет.</summary>
        /// <param name="Number">Номер сокета.</param>
        public void Close(int Number) { SocketCanLib.SocketClose(Number); }

        /// <summary>Отправляет CAN-фрейм.</summary>
        /// <param name="SocketNumber">Номер сокета для отправки.</param>
        /// <param name="Frames">Фрейм для отправки.</param>
        /// <returns>Количество сообщений, поставленых в буфер</returns>
        public int Write(int SocketNumber, CanFrame Frames)
        {
            var xx = new SocketCanFdFrame(Frames);
            SocketCanFdFrame* framesBufferPtr = &xx;
            int res = SocketCanLib.SocketWrite(SocketNumber, framesBufferPtr, 1);
            if (res >= 0) return res;
            throw new SocketCanWriteException(-res);
        }

        /// <summary>Пытается прочитать фреймы из сокета.</summary>
        /// <param name="SocketNumber">Номер сокета для чтения.</param>
        /// <param name="Timeout">Таймаут ожидания получения сообщения в случае, если во входящем буфере не оказалось сообщений.</param>
        /// <returns>Список фреймов, прочитанных из указанного сокета.</returns>
        public CanFrame Read(int SocketNumber, TimeSpan Timeout)
        {
            int result;
            var bag = new FrameBag();
            FrameBag* bagPtr = &bag;
            result = SocketCanLib.SocketRead(SocketNumber, bagPtr, 1, (int)Timeout.TotalMilliseconds);

            if (result >= 0) return GetCanFrame(bag);
            throw new SocketCanReadException(-result);
        }

        /// <summary>Выполняет отчистку буфера входящих сообщений для указанного сокета</summary>
        /// <param name="SocketNumber">Номер сокета, в котором требуется отчистить буфер входящих сообщений</param>
        public void FlushInBuffer(int SocketNumber)
        {
            int res = SocketCanLib.SocketFlushInBuffer(SocketNumber);
            if (res < 0) throw new SocketCanFlushException(-res);
        }

        private static byte[] GetCString(String str)
        {
            var cString = new byte[str.Length + 1];
            _encoder.GetBytes(str.ToCharArray(), 0, str.Length, cString, 0, true);
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
