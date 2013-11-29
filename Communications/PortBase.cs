using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Sockets;

namespace Communications
{
    public abstract class PortBase<TSocket, TDatagram> : IPort<TSocket>, ISendPipe<TDatagram>, IReceivePipe<TDatagram>
        where TSocket : ISocket<TDatagram>
    {
        public string Name { get; private set; }

        public abstract int BaudRate { get; set; }
        public event EventHandler BaudRateChanged;
        protected virtual void OnBaudRateChanged()
        {
            var handler = BaudRateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected PortBase(string Name) { this.Name = Name; }

        protected abstract void SendImplementation(IList<TDatagram> Data);
        void ISendPipe<TDatagram>.Send(IList<TDatagram> Data)
        {
            (this as IReceivePipe<TDatagram>).ProcessReceived(Data);
            SendImplementation(Data);
        }

        void IReceivePipe<TDatagram>.ProcessReceived(IList<TDatagram> Datagrams)
        {
            foreach (var socket in _openedSockets.OfType<IBufferedStore<TDatagram>>())
            {
                socket.Enqueue(Datagrams);
            }
        }

        #region Работа с хранением сокетов

        private readonly object _openedSocketsLocker = new object();
        private readonly List<TSocket> _openedSockets = new List<TSocket>();

        protected abstract TSocket CreateSocket();

        public TSocket OpenSocket()
        {
            lock (_openedSocketsLocker)
            {
                var socket = CreateSocket();
                socket.Disposed += (Sender, Args) => OnSocketDisposed((TSocket)Sender);
                _openedSockets.Add(socket);
                return socket;
            }
        }

        private void OnSocketDisposed(TSocket Socket)
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
                foreach (var socket in _openedSockets)
                {
                    socket.Dispose();
                }
            }
        }

        public event EventHandler AllSocketsDisposed;

        private void OnAllSocketsDisposed()
        {
            var handler = AllSocketsDisposed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>Проверяет, есть ли у этого источника открытые сокеты</summary>
        public bool HaveOpenedSockets
        {
            get {
                lock (_openedSocketsLocker)
                {
                    return _openedSockets.Any();
                }
            }
        }

        #endregion
        
        public void Dispose()
        {
            DisposeAllSockets();
        }
    }
}