using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Sockets;

namespace Communications
{
    public abstract class PortBase<TDatagram> : IPort<TDatagram>
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
        public void Send(IList<TDatagram> Data)
        {
            ProcessReceived(Data);
            SendImplementation(Data);
        }

        public void ProcessReceived(IList<TDatagram> Datagrams)
        {
            foreach (var socket in _openedSockets.OfType<IBufferedStore<TDatagram>>())
            {
                socket.Enqueue(Datagrams);
            }
        }

        #region Работа с хранением сокетов

        private readonly object _openedSocketsLocker = new object();
        private readonly List<ISocket<TDatagram>> _openedSockets = new List<ISocket<TDatagram>>();

        protected abstract ISocket<TDatagram> CreateSocket();

        public ISocket<TDatagram> OpenSocket()
        {
            lock (_openedSocketsLocker)
            {
                var socket = CreateSocket();
                socket.Disposed += (Sender, Args) => OnSocketDisposed(Sender as ISocket<TDatagram>);
                _openedSockets.Add(socket);
                return socket;
            }
        }

        private void OnSocketDisposed(ISocket<TDatagram> Socket)
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

        #endregion
        
        public void Dispose()
        {
            DisposeAllSockets();
        }
    }
}