using System;
using Communications.Can;

namespace Communications.SocketCan.LinuxSockets
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
        /// <param name="Frame">Сообщения для отправки</param>
        /// <returns>Количество сообщений, поставленых в буфер</returns>
        int Send(CanFrame Frame);

        /// <summary>Выполняет чтение из сокета</summary>
        /// <param name="Timeout">Таймаут операции чтения</param>
        /// <returns>Сообщения, оказавшиеся в буфере чтения на момент вызова процедуры, либо пришедшие за время, указанное в
        ///     <paramref name="Timeout" /> после её вызова.</returns>
        CanFrame Receive(TimeSpan Timeout);
    }
}
