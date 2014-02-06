using System;

namespace Communications.Piping
{
    /// <summary>
    /// Просто реализует интерфейс трубы, вызывая событие DatagramsReceived по вызову из вне
    /// </summary>
    /// <typeparam name="TDatagram"></typeparam>
    public class RedirectReceivePipe<TDatagram> : IReceivePipe<TDatagram>
    {
        public event EventHandler<DatagramsReceivedEventArgs<TDatagram>> DatagramsReceived;
        public void OnDatagramsReceived(DatagramsReceivedEventArgs<TDatagram> E)
        {
            var handler = DatagramsReceived;
            if (handler != null) handler(this, E);
        }
    }
}