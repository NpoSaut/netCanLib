using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Exceptions;

namespace Communications.Sockets
{
    /// <summary>
    /// Абстракция сокета, выполняющего преобразование дейтаграмм
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграмм на выходе</typeparam>
    /// <typeparam name="TSourceDatagram">Тип дейтаграмм на входе</typeparam>
    public abstract class ReformativeSocketBase<TDatagram, TSourceDatagram> : SocketBase<TDatagram>
    {
        /// <summary>
        /// Создаёт экземпляр преобразующего сокета
        /// </summary>
        /// <param name="Name">Имя сокета</param>
        /// <param name="OriginalSocket">Оригинальный сокет, из которого берутся данные</param>
        protected ReformativeSocketBase(string Name, ISocket<TSourceDatagram> OriginalSocket) : base(Name)
        {
            this.OriginalSocket = OriginalSocket;
        }

        /// <summary>Сокет-источник</summary>
        protected ISocket<TSourceDatagram> OriginalSocket { get; private set; }

        /// <summary>Выполняет прямое преобразование дейтаграмм источника в выходные дейтаграммы</summary>
        /// <param name="SourceDatagram">Дейтаграмма источника</param>
        /// <returns>Дейтаграмма соответствующего типа</returns>
        protected abstract TDatagram Convert(TSourceDatagram SourceDatagram);

        /// <summary>Выполняет обратное преобразование в тип дейтаграмм источника</summary>
        /// <param name="Datagram">Дейтаграмма</param>
        /// <returns>Дейтаграмма, передаваемая в источник</returns>
        protected abstract TSourceDatagram ConvertBack(TDatagram Datagram);

        /// <summary>Выполняет чтение из сокета</summary>
        /// <param name="Timeout">Таймаут чтения</param>
        /// <param name="ThrowExceptionOnTimeout">
        ///     Указывает, следует ли выбрасывать исключение
        ///     <see cref="SocketTimeoutException" /> при превышении таймаута чтения, или просто прервать считывание
        ///     последовательности
        /// </param>
        /// <returns>Последовательность считанных дейтаграмм</returns>
        public override IEnumerable<TDatagram> Receive(TimeSpan Timeout = new TimeSpan(),
                                                       bool ThrowExceptionOnTimeout = false)
        {
            return OriginalSocket.Receive(Timeout, ThrowExceptionOnTimeout).Select(Convert);
        }

        /// <summary>Отправляет дейтаграммы в сокет</summary>
        public override void Send(IEnumerable<TDatagram> Data, TimeSpan Timeout = new TimeSpan())
        {
            OriginalSocket.Send(Data.Select(ConvertBack));
        }
    }
}
