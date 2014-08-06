using System;
using System.Collections.Generic;
using System.Linq;

namespace Communications.Ports
{
    /// <summary>Базовый класс порта. Реализует работу с сокетами.</summary>
    /// <typeparam name="TSocket">Тип сокета</typeparam>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    /// <remarks>
    ///     Реализует хранение списка сокетов, сбор сообщений на отправку с сокетов и рассылку по сокетам принятых с
    ///     низлежащего уровня сообщений.
    /// </remarks>
    public abstract class PortBase<TSocket, TDatagram> : IPort<TSocket>
        where TSocket : ISocket<TDatagram>
    {
        /// <summary>Список открытых сокетов</summary>
        protected readonly List<TSocket> OpenedSockets = new List<TSocket>();

        /// <summary>Объект для блокировки при доступе к списку открытых сокетов</summary>
        protected readonly object OpenedSocketsLocker = new object();

        protected PortBase(string Name) { this.Name = Name; }

        public void Dispose() { DisposeAllSockets(); }

        /// <summary>Имя порта</summary>
        public string Name { get; private set; }

        public event EventHandler AllSocketsDisposed;

        /// <summary>Проверяет, есть ли у этого источника открытые сокеты</summary>
        public bool HaveOpenedSockets
        {
            get
            {
                lock (OpenedSocketsLocker)
                {
                    return OpenedSockets.Any();
                }
            }
        }

        /// <summary>Открывает новый сокет</summary>
        public TSocket OpenSocket()
        {
            TSocket socket = ImplementOpenSocket();
            RegisterSocket(socket);
            return socket;
        }

        /// <summary>Создаёт новый сокет, который будет зарегистрирован в порту и возвращён функцией OpenSocket</summary>
        protected abstract TSocket ImplementOpenSocket();

        protected virtual void RegisterSocket(TSocket Socket)
        {
            Socket.Closed += SocketOnClosed;
            lock (OpenedSocketsLocker)
            {
                OpenedSockets.Add(Socket);
            }
        }

        private void SocketOnClosed(object Sender, EventArgs Args) { ReleaseSocket((TSocket)Sender); }

        /// <summary>Убирает закрытый сокет из списка, проверяет не был ли он последним открытым</summary>
        private void ReleaseSocket(TSocket Socket)
        {
            lock (OpenedSocketsLocker)
            {
                OpenedSockets.Remove(Socket);
                if (!OpenedSockets.Any()) OnAllSocketsDisposed();
            }
        }

        /// <summary>Закрывает все открытые сокеты</summary>
        private void DisposeAllSockets()
        {
            lock (OpenedSocketsLocker)
            {
                foreach (TSocket socket in OpenedSockets.ToList())
                    socket.Dispose();
            }
        }

        /// <summary>Вызывает событие AllSocketsDisposed</summary>
        private void OnAllSocketsDisposed()
        {
            EventHandler handler = AllSocketsDisposed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
