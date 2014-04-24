using System;
using Communications.Exceptions;

namespace Communications.Ports
{
    /// <summary>Абстракция порта, работающего только с одним открытым сокетом</summary>
    /// <typeparam name="TSocket">Тип сокета</typeparam>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public abstract class SingleSocketPortBase<TSocket, TDatagram> : PortBase<TSocket, TDatagram>
        where TSocket : ISocket<TDatagram>
    {
        private readonly object _openedSocketLocker = new object();
        private TSocket _openedSocket;

        protected SingleSocketPortBase(string Name) : base(Name) { }

        public TSocket OpenedSocket
        {
            get
            {
                lock (_openedSocketLocker)
                {
                    return _openedSocket;
                }
            }
            private set
            {
                lock (_openedSocketLocker)
                {
                    _openedSocket = value;
                }
            }
        }

        /// <summary>Проверяет, есть ли у этого источника открытые сокеты</summary>
        public override bool HaveOpenedSockets
        {
            get { return OpenedSocket != null; }
        }

        protected override void OnRegisterSocket(TSocket Socket)
        {
            lock (_openedSocketLocker)
            {
                if (HaveOpenedSockets) throw new SingleSocketReopenException(Name);
                OpenedSocket = Socket;
            }
            base.OnRegisterSocket(Socket);
        }

        protected override void OnReleaseSocket(TSocket Socket)
        {
            OpenedSocket = default(TSocket);
            base.OnReleaseSocket(Socket);
        }

        /// <summary>
        ///     Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых
        ///     ресурсов.
        /// </summary>
        public override void Dispose()
        {
            lock (_openedSocketLocker)
            {
                if (HaveOpenedSockets)
                {
                    OpenedSocket.Dispose();
                }
            }
            base.Dispose();
        }
    }
}
