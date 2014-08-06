using System.Collections.Generic;

namespace Communications.Ports
{
    /// <summary>Порт, связанный с низлежащими уровнями посредством труб</summary>
    /// <typeparam name="TSocket">Тип сокета</typeparam>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public abstract class PipedPortBase<TSocket, TDatagram> : BackendPortBase<TSocket, TDatagram>
        where TSocket : ISocket<TDatagram>, ISocketBackend<TDatagram>
    {
        /// <summary>Создаёт экземпляр порта, работающего с нижнем уровнем посредством труб</summary>
        /// <param name="Name">Имя порта</param>
        /// <param name="SendPipe">Труба, в которую будут переданы отправляемые сообщения из сокетов</param>
        /// <param name="ReceivePipe">Труба, из которой ожидаются входящие сообщения</param>
        public PipedPortBase(string Name, ISendPipe<TDatagram> SendPipe, IReceivePipe<TDatagram> ReceivePipe) : base(Name)
        {
            this.SendPipe = SendPipe;
            this.ReceivePipe = ReceivePipe;
            ReceivePipe.DatagramsReceived += ReceivePipeOnDatagramsReceived;
        }

        /// <summary>Труба для отправки дейтаграмм</summary>
        public ISendPipe<TDatagram> SendPipe { get; private set; }

        /// <summary>Труба для приёма дейтаграмм</summary>
        public IReceivePipe<TDatagram> ReceivePipe { get; private set; }

        private void ReceivePipeOnDatagramsReceived(object Sender, DatagramsReceivedEventArgs<TDatagram> e) { OnDatagramsReceived(e.Datagrams); }

        /// <summary>Реализует отправку принятых из сокета дейтаграмм.</summary>
        protected override void SendingImplementation(ISocketBackend<TDatagram> Source, IList<TDatagram> Data) { SendPipe.Send(Data); }
    }
}
