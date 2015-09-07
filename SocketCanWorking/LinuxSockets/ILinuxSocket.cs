using System;
using System.Collections.Generic;
using Communications.Can;

namespace SocketCanWorking.LinuxSockets
{
    /// <summary>Linux Can сокет</summary>
    /// <remarks>Реализует открытие сокета, получение и хранение его номера, а так же запись и чтение по своему номеру сокета</remarks>
    public interface ILinuxSocket : IDisposable
    {
        /// <summary>Размер буфера входящих сообщений</summary>
        int RxBufferSize { get; }

        /// <summary>Размер буфера исходящих сообщений</summary>
        int TxBufferSize { get; }

        /// <summary>Смывает буфер входящих сообщений</summary>
        void FlushInBuffer();

        /// <summary>Ставит сообщения в очередь на отправку в SocketCan</summary>
        /// <param name="Frames">Сообщения для отправки</param>
        /// <returns>Количество сообщений, поставленых в буфер</returns>
        int Send(IList<CanFrame> Frames);

        /// <summary>Выполняет чтение из сокета</summary>
        /// <param name="Timeout">Таймаут операции чтения</param>
        /// <returns>Сообщения, оказавшиеся в буфере чтения на момент вызова процедуры, либо пришедшие за время, указанное в
        ///     <paramref name="Timeout" /> после её вызова.</returns>
        IList<CanFrame> Receive(TimeSpan Timeout);
    }
}
