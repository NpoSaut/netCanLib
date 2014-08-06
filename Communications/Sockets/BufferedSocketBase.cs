using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Communications.Exceptions;

namespace Communications.Sockets
{
    /// <summary>Абстракция сокета, буферизирующего все входящие дейтаграммы до момента их прочтения</summary>
    /// <remarks>
    ///     Разделяет процессы накладывания сообщений в себя и изъятия. Не препятствует помещению дейтаграмм в буфер,
    ///     позволяя сделать это как можно быстрее. При изъятии сообщений выдаёт сообщение из буфера либо блокирует вызов до
    ///     появление в буфере сообщений.
    /// </remarks>
    public abstract class BufferedSocketBase<TDatagram> : BackendSocketBase<TDatagram>
    {
        private readonly IDatagramBuffer<TDatagram> _buffer;

        protected BufferedSocketBase(string Name) : this(Name, new ConcurrentDatagramBuffer<TDatagram>()) { }
        protected BufferedSocketBase(string Name, IDatagramBuffer<TDatagram> Buffer) : base(Name) { _buffer = Buffer; }

        /// <summary>Добавляет датаграммы в очередь на обработку</summary>
        /// <param name="Datagrams">Полученные датаграммы</param>
        public override void ProcessReceivedDatagrams(IEnumerable<TDatagram> Datagrams)
        {
            _buffer.Enqueue(Datagrams);
        }

        protected override IEnumerable<TDatagram> ImplementReceive(TimeSpan Timeout)
        {
            return _buffer.Read(Timeout);
        }
    }

    public interface IDatagramBuffer<TDatagram>
    {
        void Enqueue(IEnumerable<TDatagram> Datagrams);
        IEnumerable<TDatagram> Read(TimeSpan Timeout = default(TimeSpan));
    }

    public class ConcurrentDatagramBuffer<TDatagram> : IDatagramBuffer<TDatagram>
    {
        private readonly ConcurrentQueue<TDatagram> _incomingDatagrams = new ConcurrentQueue<TDatagram>();

        public void Enqueue(IEnumerable<TDatagram> Datagrams)
        {
            foreach (TDatagram datagram in Datagrams)
                _incomingDatagrams.Enqueue(datagram);
        }

        public IEnumerable<TDatagram> Read(TimeSpan Timeout = default(TimeSpan))
        {
            if (Timeout == TimeSpan.Zero) Timeout = TimeSpan.MaxValue;
            while (true)
            {
                TDatagram dtg = default(TDatagram);
                bool ok = SpinWait.SpinUntil(() => _incomingDatagrams.TryDequeue(out dtg), Timeout);

                if (ok) yield return dtg;
                else throw new SocketReadTimeoutException("Превышено время ожидания дейтаграммы");
            }
        }
    }
}
