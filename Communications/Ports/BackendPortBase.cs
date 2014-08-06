using System.Collections.Generic;

namespace Communications.Ports
{
    public abstract class BackendPortBase<TSocket, TDatagram> : PortBase<TSocket, TDatagram>
        where TSocket : ISocket<TDatagram>, ISocketBackend<TDatagram>
    {
        protected BackendPortBase(string Name) : base(Name) { }

        protected override void RegisterSocket(TSocket Socket)
        {
            base.RegisterSocket(Socket);
            RegisterSocketBackend(Socket);
        }

        private void RegisterSocketBackend(ISocketBackend<TDatagram> Backend)
        {
            Backend.SendingRequested += (Sender, Args) => SendingImplementation((ISocketBackend<TDatagram>)Sender, Args.Datagrams);
        }

        /// <summary>Реализует отправку принятых из сокета дейтаграмм.</summary>
        protected abstract void SendingImplementation(ISocketBackend<TDatagram> Source, IList<TDatagram> Data);

        /// <summary>Реализует действия, необходимые для рассылки принятых дейтаграмм по сокетам</summary>
        protected void OnDatagramsReceived(IList<TDatagram> Datagrams)
        {
            lock (OpenedSocketsLocker)
            {
                foreach (TSocket socketBackend in OpenedSockets)
                    socketBackend.ProcessReceivedDatagrams(Datagrams);
            }
        }
    }
}