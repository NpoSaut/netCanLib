using System;
using System.Collections.Generic;

namespace Communications
{
    /// <summary>
    /// Интерфейс сокета
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы сокета</typeparam>
    public interface ISocket<TDatagram> : IDisposable
    {
        /// <summary>Имя сокета</summary>
        String Name { get; }

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
        IEnumerable<TDatagram> Read(TimeSpan Timeout = default(TimeSpan), bool ThrowExceptionOnTimeOut = false);
    }
}