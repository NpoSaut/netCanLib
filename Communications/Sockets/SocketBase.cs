using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Exceptions;

namespace Communications.Sockets
{
    /// <summary>Базовая абстракция сокета</summary>
    /// <typeparam name="TDatagram">Тип дейтаграмм, которыми оперирует данный сокет</typeparam>
    public abstract class SocketBase<TDatagram> : ISocket<TDatagram>
    {
        protected SocketBase(string Name)
        {
            this.Name = Name;
            IsOpened = true;
        }

        /// <summary>Имя сокета</summary>
        public string Name { get; private set; }

        /// <summary>Проверяет, является ли сокет открытым</summary>
        public bool IsOpened { get; private set; }

        /// <summary>Отправляет дейтаграммы в сокет</summary>
        public abstract void Send(IEnumerable<TDatagram> Data); // TODO: Проверка на закрытость сокета

        /// <summary>Отправляет дейтаграмму в сокет</summary>
        public virtual void Send(TDatagram Data) { Send(new[] { Data }); }

        /// <summary>Возникает при закрытии сокета</summary>
        public virtual event EventHandler Closed;

        /// <summary>Выполняет чтение из сокета</summary>
        /// <param name="Timeout">Таймаут чтения</param>
        /// <param name="ThrowExceptionOnTimeout">
        ///     Указывает, следует ли выбрасывать исключение
        ///     <see cref="SocketTimeoutException" /> при превышении таймаута чтения, или просто прервать считывание
        ///     последовательности
        /// </param>
        /// <returns>Последовательность считанных дейтаграмм</returns>
        public abstract IEnumerable<TDatagram> Receive(TimeSpan Timeout = new TimeSpan(),
                                                       bool ThrowExceptionOnTimeout = false);

        public virtual void Dispose()
        {
            if (IsOpened)
            {
                IsOpened = false;
                EventHandler handler = Closed;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }

        /// <summary>Отправляет дейтаграммы в сокет</summary>
        public virtual void Send(params TDatagram[] Data) { Send(Data.AsEnumerable()); }
    }
}