using System;
using System.Collections.Generic;
using System.Linq;

namespace Communications.Sockets
{
    /// <summary>Сокет, который реализует работу с источником данных через <see cref="ISocketBackend{TDatagram}" />
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public abstract class BackendSocketBase<TDatagram> : SocketBase<TDatagram>, ISocketBackend<TDatagram>
    {
        protected BackendSocketBase(string Name) : base(Name) { }

        /// <summary>Сюда передавать принятые с нижлежащего уровня сообщения</summary>
        public abstract void ProcessReceivedDatagrams(IEnumerable<TDatagram> Datagrams);

        public event EventHandler<SendRequestedEventArgs<TDatagram>> SendingRequested;
        protected override void ImplementSend(IEnumerable<TDatagram> Datagrams) { RequestSending(Datagrams); }

        protected void RequestSending(IEnumerable<TDatagram> Datagrams)
        {
            EventHandler<SendRequestedEventArgs<TDatagram>> handler = SendingRequested;
            if (handler != null) handler(this, new SendRequestedEventArgs<TDatagram>(Datagrams.ToList()));
        }
    }
}
