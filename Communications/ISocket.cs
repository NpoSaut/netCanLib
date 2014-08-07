using System;
using System.Collections.Generic;
using Communications.Exceptions;

namespace Communications
{
    /// <summary>Интерфейс сокета</summary>
    public interface ISocket : INamable, IClosable { }

    /// <summary>Интерфейс сокета</summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы сокета</typeparam>
    public interface ISocket<TDatagram> : ISocket
    {
        /// <summary>Отправляет дейтаграмму в сокет</summary>
        void Send(TDatagram Data);

        /// <summary>Отправляет дейтаграммы в сокет</summary>
        void Send(IEnumerable<TDatagram> Data);

        /// <summary>Производит блокирующее считывание данных из сокета</summary>
        /// <param name="Timeout">Таймаут на операцию считывания</param>
        /// <param name="ThrowExceptionOnTimeout">
        ///     Если true, то при превышении времени ожидания очередного сообщения будет
        ///     выбрасываться <see cref="SocketReadTimeoutException" />, если нет -- просто будет перечисление коллекции будет
        ///     прекращено
        /// </param>
        /// <returns>Последовательность считанных дейтаграмм</returns>
        /// <remarks>
        ///     Поскольку функция блокирующая и считывает _все_ приходящие кадры, выход из этого перечисления не предусмотрен.
        ///     Для правильной работы следует пользоваться лишь методами вроде методов, извлекающих N первых членов
        ///     последовательности (First, Take, TakeWhile, ...) Можно использовать для отлова серий подряд идущих сообщений (когда
        ///     другая серия отделяется по времени). Параметр ThrowExceptionOnTimeout следует использовать, если сообщение
        ///     гарантированно должно поступить и его задержка является признаком ошибки
        /// </remarks>
        /// <exception cref="SocketReadTimeoutException">
        ///     При превышении времени ожидания сообщения (если
        ///     <paramref name="ThrowExceptionOnTimeout" /> == true)
        /// </exception>
        IEnumerable<TDatagram> Receive(TimeSpan Timeout, bool ThrowExceptionOnTimeout = false);

        /// <summary>Производит блокирующее считывание данных из сокета</summary>
        /// <returns>Последовательность считанных дейтаграмм</returns>
        /// <remarks>
        ///     Поскольку функция блокирующая и считывает _все_ приходящие кадры, выход из этого перечисления не предусмотрен.
        ///     Для правильной работы следует пользоваться лишь методами вроде методов, извлекающих N первых членов
        ///     последовательности (First, Take, TakeWhile, ...)
        /// </remarks>
        IEnumerable<TDatagram> Receive();
    }
}
