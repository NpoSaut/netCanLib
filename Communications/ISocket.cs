using System;
using System.Collections.Generic;

namespace Communications
{
    /// <summary>
    /// Интерфейс сокета
    /// </summary>
    public interface ISocket : IDisposable
    {
        /// <summary>Имя сокета</summary>
        String Name { get; }
        /// <summary>Событие, возникающее при уничтожении сокета</summary>
        event EventHandler Disposed;
    }

    /// <summary>
    /// Интерфейс сокета
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы сокета</typeparam>
    public interface ISocket<TDatagram> : ISocket
    {
        /// <summary>Отправляет дейтаграмму в сокет</summary>
        void Send(TDatagram Data);
        /// <summary>Отправляет дейтаграммы в сокет</summary>
        void Send(IEnumerable<TDatagram> Data);

        /// <summary>
        /// Производит блокирующее считывание данных из сокета
        /// </summary>
        /// <param name="Timeout">Таймаут на операцию считывания</param>
        /// <param name="ThrowExceptionOnTimeOut">Указывает, следует ли вызывать исключение при превышении таймаута</param>
        /// <returns>Последовательность считанных дейтаграмм</returns>
        /// <remarks>
        /// Поскольку функция блокирующая и считывает _все_ приходящие кадры, выход из этого перечисления не предусмотрен.
        /// Для правильной работы следует пользоваться лишь методами вроде методов, извлекающих N первых членов последовательности (First, Take, TakeWhile, ...)
        /// 
        /// Можно использовать для отлова серий подряд идущих сообщений (когда другая серия отделяется по времени).
        /// Параметр ThrowExceptionOnTimeout следует использовать, если сообщение гарантированно должно поступить и его задержка является признаком ошибки
        /// </remarks>
        IEnumerable<TDatagram> Read(TimeSpan Timeout = default(TimeSpan), bool ThrowExceptionOnTimeOut = false);
    }
}