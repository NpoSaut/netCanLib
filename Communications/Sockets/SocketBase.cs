using System;
using System.Collections.Generic;
using Communications.Exceptions;
using Communications.Tools;

namespace Communications.Sockets
{
    /// <summary>Базовая абстракция сокета</summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public abstract class SocketBase<TDatagram> : ISocket<TDatagram>
    {
        protected SocketBase(string Name)
        {
            this.Name = Name;
            IsOpened = true;
        }

        /// <summary>Имя сокета</summary>
        public virtual string Name { get; private set; }

        /// <summary>Производит блокирующее чтение из сокета</summary>
        /// <param name="Timeout">Таймаут ожидания очередного пакета</param>
        /// <param name="ThrowExceptionOnTimeout">
        ///     Если true, то при превышении времени ожидания очередного сообщения будет
        ///     выбрасываться <see cref="SocketReadTimeoutException" />, если нет -- просто будет перечисление коллекции будет
        ///     прекращено
        /// </param>
        /// <exception cref="SocketReadTimeoutException">Превышено время ожидания очередного сообщения</exception>
        public IEnumerable<TDatagram> Receive(TimeSpan Timeout, bool ThrowExceptionOnTimeout)
        {
            if (!IsOpened) throw new SocketClosedException();
            IEnumerable<TDatagram> datagramsFlow = ImplementReceive(Timeout);
            if (!ThrowExceptionOnTimeout) datagramsFlow = datagramsFlow.SuppressExceptions<TDatagram, SocketReadTimeoutException>();
            return datagramsFlow;
        }

        /// <summary>Производит блокирующее считывание данных из сокета</summary>
        /// <returns>Последовательность считанных дейтаграмм</returns>
        /// <remarks>
        ///     Поскольку функция блокирующая и считывает _все_ приходящие кадры, выход из этого перечисления не предусмотрен.
        ///     Для правильной работы следует пользоваться лишь методами вроде методов, извлекающих N первых членов
        ///     последовательности (First, Take, TakeWhile, ...)
        /// </remarks>
        public IEnumerable<TDatagram> Receive()
        {
            if (!IsOpened) throw new SocketClosedException();
            return ImplementReceive();
        }

        public virtual void Send(TDatagram Data) { Send(new[] { Data }); }

        public void Send(IEnumerable<TDatagram> Data)
        {
            if (!IsOpened) throw new SocketClosedException();
            ImplementSend(Data);
        }

        #region IDisposable

        public event EventHandler Closed;

        /// <summary>Показывает, открыт ли сокет</summary>
        public bool IsOpened { get; private set; }

        public virtual void Dispose()
        {
            if (IsOpened)
            {
                IsOpened = false;
                EventHandler handler = Closed;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }

        #endregion

        protected abstract void ImplementSend(IEnumerable<TDatagram> Datagrams);
        protected abstract IEnumerable<TDatagram> ImplementReceive(TimeSpan Timeout);
        protected virtual IEnumerable<TDatagram> ImplementReceive() { return ImplementReceive(TimeSpan.MaxValue); }
    }
}
