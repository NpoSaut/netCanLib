﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Communications.Sockets
{
    public abstract class SocketBase<TDatagram> : ISocket<TDatagram>
    {
        public virtual string Name { get; private set; }

        protected SocketBase(string Name) { this.Name = Name; }

        public abstract void Send(IEnumerable<TDatagram> Data);
        public abstract IEnumerable<TDatagram> Read(TimeSpan Timeout = new TimeSpan(), bool ThrowExceptionOnTimeOut = false);

        public void Send(params TDatagram[] Data) { Send(Data.AsEnumerable()); }
        public virtual void Send(TDatagram Data) { Send(new[] { Data }); }

        public event EventHandler Disposed;
        private void OnDisposed()
        {
            var handler = Disposed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            OnDisposed();
        }
    }
}