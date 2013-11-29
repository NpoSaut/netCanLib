using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Communications
{
    /// <summary>
    /// Абстрактный интерфейс порта заданный дейтаграмм
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public interface IPort<TDatagram> : ISocketSource<TDatagram>, IDisposable
    {
        /// <summary>Имя порта</summary>
        string Name { get; }
        /// <summary>Скорость порта</summary>
        int BaudRate { get; set; }
        /// <summary>Скорость порта была изменена</summary>
        event EventHandler BaudRateChanged;
    }

    /// <summary>
    /// Объект, имеющий в своём распоряжении некоторые сокеты
    /// </summary>
    public interface ISocketContainer : IDisposable
    {
        /// <summary>Все сокеты, открытые на этом порту были закрыты</summary>
        event EventHandler AllSocketsDisposed;

        /// <summary>Проверяет, есть ли у этого источника открытые сокеты</summary>
        bool HaveOpenedSockets { get; }
    }

    /// <summary>
    /// Источник сокетов. Может открыть сокеты и вести учёт их закрытия
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public interface ISocketSource<TDatagram> : ISocketContainer
    {
        /// <summary>Открывает сокет на данном порту</summary>
        /// <returns>Свежеоткрытый сокет</returns>
        ISocket<TDatagram> OpenSocket();
    }

    /// <summary>
    /// Объект, способный обработать заказ на отправку дейтаграмм
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public interface ISendPipe<TDatagram>
    {
        void Send(IList<TDatagram> Data);
    }

    /// <summary>
    /// Объект, способный обработать заказ на обработку принятых дейтаграмм
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public interface IReceivePipe<TDatagram>
    {
        void ProcessReceived(IList<TDatagram> Datagrams);
    }
}