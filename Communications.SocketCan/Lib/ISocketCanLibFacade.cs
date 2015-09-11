using System;
using Communications.Can;
using Communications.SocketCan.Exceptions;

namespace Communications.SocketCan.Lib
{
    /// <summary>Обёртка над библиотечными функциями, скрывающая подробности вызова c-шных функций и работу с указателями</summary>
    public interface ISocketCanLibFacade
    {
        /// <summary>Открывает сокет.</summary>
        /// <param name="InterfaceName">Имя сокета</param>
        /// <param name="RxBuffSize">Размер буфера входящих сообщений</param>
        /// <param name="TxBuffSize">Размер буфера исходящих сообщений</param>
        /// <exception cref="SocketCanOpenException">Ошибка при попытке открыть сокет.</exception>
        int Open(string InterfaceName, int RxBuffSize, int TxBuffSize);

        /// <summary>Закрывает сокет.</summary>
        /// <param name="Number">Номер сокета.</param>
        void Close(int Number);

        /// <summary>Отправляет CAN-фрейм.</summary>
        /// <param name="SocketNumber">Номер сокета для отправки.</param>
        /// <param name="Frames">Фрейм для отправки.</param>
        /// <returns>Количество сообщений, поставленых в буфер</returns>
        int Write(int SocketNumber, CanFrame Frames);

        /// <summary>Пытается прочитать фреймы из сокета.</summary>
        /// <param name="SocketNumber">Номер сокета для чтения.</param>
        /// <param name="Timeout">Таймаут ожидания получения сообщения в случае, если во входящем буфере не оказалось сообщений.</param>
        /// <returns>Список фреймов, прочитанных из указанного сокета.</returns>
        CanFrame Read(int SocketNumber, TimeSpan Timeout);

        /// <summary>Выполняет отчистку буфера входящих сообщений для указанного сокета</summary>
        /// <param name="SocketNumber">Номер сокета, в котором требуется отчистить буфер входящих сообщений</param>
        void FlushInBuffer(int SocketNumber);
    }
}
