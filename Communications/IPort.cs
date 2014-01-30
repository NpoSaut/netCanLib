﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Communications
{
    /// <summary>
    /// Абстрактный интерфейс порта заданный дейтаграмм
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public interface IPort<TSocket> : ISocketSource<TSocket>, INamable
        where TSocket : ISocket
    { }

    /// <summary>
    /// Объект, имеющий в своём распоряжении некоторые сокеты
    /// </summary>
    public interface ISocketOwner : IDisposable
    {
        /// <summary>Все сокеты, открытые на этом порту были закрыты</summary>
        event EventHandler AllSocketsDisposed;

        /// <summary>Проверяет, есть ли у этого источника открытые сокеты</summary>
        bool HaveOpenedSockets { get; }
    }

    /// <summary>
    /// Источник сокетов. Может открыть сокеты и вести учёт их закрытия
    /// </summary>
    public interface ISocketSource<TSocket> : ISocketOwner
    {
        /// <summary>Открывает сокет на данном порту</summary>
        /// <returns>Свежеоткрытый сокет</returns>
        TSocket OpenSocket();
    }
}