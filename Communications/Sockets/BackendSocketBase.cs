using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Exceptions;

namespace Communications.Sockets
{
    /// <summary>Абстракция сокета, реализующего взаимодействие с портом через Backend-систему</summary>
    /// <typeparam name="TDatagram">Тип дейтаграмм, которыми оперирует данный сокет</typeparam>
    public abstract class BackendSocketBase<TDatagram> : SocketBase<TDatagram>, ISocketBackend<TDatagram>
    {
        protected BackendSocketBase(string Name) : base(Name) { }

        public event EventHandler<SendRequestedEventArgs<TDatagram>> SendingRequested;

        /// <summary>Сюда передавать принятые с нижлежащего уровня сообщения</summary>
        public abstract void ProcessReceivedDatagrams(IEnumerable<TDatagram> Datagrams);

        public override void Send(IEnumerable<TDatagram> Data, TimeSpan Timeout = default(TimeSpan))
        {
            if (!IsOpened) throw new SocketClosedException();
            RequestSending(Data, Timeout);
        }

        protected void RequestSending(IEnumerable<TDatagram> Datagrams, TimeSpan Timeout)
        {
            EventHandler<SendRequestedEventArgs<TDatagram>> handler = SendingRequested;
            if (handler != null) handler(this, new SendRequestedEventArgs<TDatagram>(Datagrams.ToList()));
        }
    }
}
