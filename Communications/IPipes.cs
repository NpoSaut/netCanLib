using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications
{
    /// <summary>Труба на отправку дейтаграмм</summary>
    /// <remarks>Реализует механизм доставки отправленных дейтаграмм на нижлежащие уровни</remarks>
    public interface ISendPipe<TDatagram>
    {
        /// <summary>
        /// Передаёт дейтаграммы на низлежащий уровень для его отправки
        /// </summary>
        void Send(IList<TDatagram> Datagrams);
    }

    /// <summary>Труба на приём дейтаграмм</summary>
    /// <remarks>Реализует механизм доставки принятых дейтаграмм с нижлежащих уровней</remarks>
    public interface IReceivePipe<TDatagram>
    {
        /// <summary>
        /// Событие, возникающее по приходу дейтаграмм с нижлежащего уровня
        /// </summary>
        event EventHandler<DatagramsReceivedEventArgs<TDatagram>> DatagramsReceived;
    }

    public class DatagramsReceivedEventArgs<TDatagram> : EventArgs
    {
        /// <summary>Принятые дейтаграммы</summary>
        public IList<TDatagram> Datagrams { get; private set; }
        public DatagramsReceivedEventArgs(IList<TDatagram> Datagrams) { this.Datagrams = Datagrams; }
    }
}
