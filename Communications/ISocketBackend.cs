using System;
using System.Collections.Generic;

namespace Communications
{
    /// <summary>
    /// Интерфейс взаимодействия сокета с портом
    /// </summary>
    /// <typeparam name="TDatagram">Тип датаграммы</typeparam>
    public interface ISocketBackend<TDatagram> : IClosable
    {
        /// <summary>
        /// Событие вызывается при необходимости передать сообщение для отправки на
        /// низлежащий уровень
        /// </summary>
        event EventHandler<SendRequestedEventArgs<TDatagram>> SendingRequested;
        /// <summary>Сюда передавать принятые с нижлежащего уровня сообщения</summary>
        void ProcessReceivedDatagrams(IEnumerable<TDatagram> Datagrams);
    }

    public class SendRequestedEventArgs<TDatagram> : EventArgs
    {
        public IList<TDatagram> Datagrams { get; private set; }
        public SendRequestedEventArgs(IList<TDatagram> Datagrams) { this.Datagrams = Datagrams; }
    }
}
