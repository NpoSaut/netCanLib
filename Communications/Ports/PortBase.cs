using System;

namespace Communications.Ports
{
    public abstract class PortBase<TSocket, TDatagram> : IPort<TSocket>
        where TSocket : ISocket<TDatagram>
    {
        protected PortBase(string Name) { this.Name = Name; }

        /// <summary>Имя порта</summary>
        public string Name { get; private set; }

        /// <summary>Открывает сокет на данном порту</summary>
        /// <remarks>Класс-наследник реализует создание сокета и вызывает его регистрацию</remarks>
        /// <returns>Свежеоткрытый сокет</returns>
        public TSocket OpenSocket()
        {
            TSocket socket = InternalOpenSocket();
            OnRegisterSocket(socket);
            return socket;
        }

        /// <summary>
        ///     Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых
        ///     ресурсов.
        /// </summary>
        public virtual void Dispose() { }

        /// <summary>Проверяет, есть ли у этого источника открытые сокеты</summary>
        public abstract bool HaveOpenedSockets { get; }

        /// <summary>Событие возникает при закрытии всех открытых на этом порту сокетов</summary>
        public event EventHandler AllSocketsDisposed;

        protected abstract TSocket InternalOpenSocket();

        /// <summary>Следует вызвать при закрытии последнего открытого сокета</summary>
        protected virtual void OnAllSocketsDisposed()
        {
            EventHandler handler = AllSocketsDisposed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void SocketOnClosed(object Sender, EventArgs Args) { OnReleaseSocket((TSocket)Sender); }

        /// <summary>Вызывается каждый раз при открытии нового порта</summary>
        protected virtual void OnRegisterSocket(TSocket Socket) { Socket.Closed += SocketOnClosed; }

        /// <summary>Вызывается каждый раз при закрытии порта</summary>
        protected virtual void OnReleaseSocket(TSocket Socket) { Socket.Closed -= SocketOnClosed; }
    }
}
