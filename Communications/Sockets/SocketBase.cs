using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Exceptions;

namespace Communications.Sockets
{
    public abstract class SocketBase<TDatagram> : ISocket<TDatagram>, ISocketBackend<TDatagram>
    {
        public virtual string Name { get; private set; }

        protected SocketBase(string Name) { this.Name = Name; }

        public void Send(IEnumerable<TDatagram> Data)
        {
            if (!IsOpened) throw new SocketClosedException();
            RequestSending(Data);
        }
        public abstract IEnumerable<TDatagram> Receive(TimeSpan Timeout = new TimeSpan(), bool ThrowExceptionOnTimeout = false); // TODO: Проверка на закрытость сокета

        public virtual void Send(params TDatagram[] Data) { Send(Data.AsEnumerable()); }
        public virtual void Send(TDatagram Data) { Send(new[] { Data }); }

        #region IDisposable
        public event EventHandler Closed;
        public bool IsOpened { get; private set; }

        public virtual void Dispose()
        {
            if (IsOpened)
            {
                IsOpened = false;
                var handler = Closed;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }
        #endregion

        public event EventHandler<SendRequestedEventArgs<TDatagram>> SendingRequested;

        protected void RequestSending(IEnumerable<TDatagram> Datagrams)
        {
            var handler = SendingRequested;
            if (handler != null) handler(this, new SendRequestedEventArgs<TDatagram>(Datagrams.ToList()));
        }

        /// <summary>Сюда передавать принятые с нижлежащего уровня сообщения</summary>
        public abstract void ProcessReceivedDatagrams(IEnumerable<TDatagram> Datagrams);
    }
}