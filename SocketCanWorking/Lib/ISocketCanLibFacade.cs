using System;
using System.Collections.Generic;
using Communications.Can;
using SocketCanWorking.Exceptions;

namespace SocketCanWorking.Lib
{
    /// <summary>Обёртка над библиотечными функциями, скрывающая подробности вызова c-шных функций и работу с указателями</summary>
    public interface ISocketCanLibFacade
    {
        /// <summary>Открывает сокет.</summary>
        /// <param name="InterfaceName">Имя сокета в виде c-строки.</param>
        /// <exception cref="SocketCanOpenException">Ошибка при попытке открыть сокет.</exception>
        int Open(String InterfaceName);

        /// <summary>Закрывает сокет.</summary>
        /// <param name="Number">Номер сокета.</param>
        void Close(int Number);

        /// <summary>Отправляет CAN-фрейм.</summary>
        /// <param name="SocketNumber">Номер сокета для отправки.</param>
        /// <param name="Frame">Фрейм для отправки.</param>
        void Write(int SocketNumber, IList<CanFrame> Frame);

        /// <summary>Пытается прочитать фреймы из сокета.</summary>
        /// <param name="SocketNumber">Номер сокета для чтения.</param>
        /// <param name="Timeout">Таймаут ожидания получения сообщения в случае, если во входящем буфере не оказалось сообщений.</param>
        /// <returns>Список фреймов, прочитанных из указанного сокета.</returns>
        IList<CanFrame> Read(int SocketNumber, TimeSpan Timeout);

        /// <summary>Выполняет отчистку буфера входящих сообщений для указанного сокета</summary>
        /// <param name="SocketNumber">Номер сокета, в котором требуется отчистить буфер входящих сообщений</param>
        void FlushInBuffer(int SocketNumber);
    }
}
