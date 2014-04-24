using System;
using System.Collections.Generic;
using System.Linq;

namespace Communications.Ports
{
    /// <summary>
    /// Абстракция порта, работающего с несколькими сокетами
    /// </summary>
    /// <typeparam name="TSocket">Тип сокета</typeparam>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    /// <remarks>Реализует хранение списка сокетов, сбор сообщений на отправку с сокетов и рассылку по сокетам принятых с низлежащего уровня сообщений.</remarks>
    public abstract class MultiSocketPortBase<TSocket, TDatagram> : PortBase<TSocket, TDatagram>
        where TSocket : ISocket<TDatagram>
    {
        protected MultiSocketPortBase(string Name) : base(Name) { }

        #region Взаимодействие с реализацией порта

        /// <summary>Реализует отправку принятых из сокета дейтаграмм.</summary>
        protected abstract void SendingImplementation(ISocketBackend<TDatagram> Source, IList<TDatagram> Data);

        /// <summary>Реализует действия, необходимые для рассылки принятых дейтаграмм по сокетам</summary>
        protected void OnDatagramsReceived(IList<TDatagram> Datagrams)
        {
            lock (_openedSocketsLocker)
            {
                foreach (var socketBackend in _openedSockets)
                    socketBackend.ProcessReceivedDatagrams(Datagrams);
            }
        }

        #endregion

        #region Работа с хранением сокетов

        private readonly object _openedSocketsLocker = new object();
        private readonly List<ISocketBackend<TDatagram>> _openedSockets = new List<ISocketBackend<TDatagram>>();

        /// <summary>"Регистрирует" сокет - добавляет в список и подписывается на события</summary>
        protected void RegisterSocketBackend(ISocketBackend<TDatagram> SocketBackend)
        {
            SocketBackend.Closed += (Sender, Args) => ReleaseSocketBackend(SocketBackend);
            SocketBackend.SendingRequested += (Sender, Args) => SendingImplementation((ISocketBackend<TDatagram>)Sender, Args.Datagrams);
            lock (_openedSocketsLocker)
            {
                _openedSockets.Add(SocketBackend);
            }
        }

        private void ReleaseSocketBackend(ISocketBackend<TDatagram> Socket)
        {
            lock (_openedSocketsLocker)
            {
                _openedSockets.Remove(Socket);
                if (!_openedSockets.Any()) OnAllSocketsDisposed();
            }
        }

        private void DisposeAllSockets()
        {
            lock (_openedSocketsLocker)
            {
                foreach (var socket in _openedSockets.ToList())
                {
                    socket.Dispose();
                }
            }
        }

        /// <summary>Проверяет, есть ли у этого источника открытые сокеты</summary>
        public override bool HaveOpenedSockets
        {
            get
            {
                lock (_openedSocketsLocker)
                {
                    return _openedSockets.Any();
                }
            }
        }

        #endregion
        
        public override void Dispose()
        {
            DisposeAllSockets();
        }
    }
}