using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Communications.Sockets
{
    /// <summary>
    /// Абстракция сокета, буферизирующего все входящие дейтаграммы до момента их прочтения
    /// </summary>
    public abstract class BufferedSockedBase<TDatagram> : SocketBase<TDatagram>, IBufferedStore<TDatagram>
    {
        private readonly IDatagramBuffer<TDatagram> _buffer;

        protected BufferedSockedBase(string Name) : this(Name, new ConcurrentDatagramBuffer<TDatagram>()) { }
        protected BufferedSockedBase(string Name, IDatagramBuffer<TDatagram> Buffer) : base(Name) { _buffer = Buffer; }

        /// <summary>
        /// Добавляет датаграммы в очередь на обработку
        /// </summary>
        /// <param name="Datagrams">Полученные датаграммы</param>
        void IBufferedStore<TDatagram>.Enqueue(IEnumerable<TDatagram> Datagrams)
        {
            _buffer.Enqueue(Datagrams);
        }

        /// <summary>
        /// Выполняет блокирующее считывание дейтаграммы из входящего потока до тех пор, пока время между соседними дейтаграммами не превысит указанный таймаут
        /// </summary>
        public override IEnumerable<TDatagram> Read(TimeSpan Timeout = default(TimeSpan), bool ThrowExceptionOnTimeOut = false)
        {
            return _buffer.Read(Timeout, ThrowExceptionOnTimeOut);
        }
    }

    public interface IDatagramBuffer<TDatagram>
    {
        void Enqueue(IEnumerable<TDatagram> Datagrams);
        IEnumerable<TDatagram> Read(TimeSpan Timeout = default(TimeSpan), bool ThrowExceptionOnTimeOut = false);
    }

    public class ConcurrentDatagramBuffer<TDatagram> : IDatagramBuffer<TDatagram>
    {
        private readonly ConcurrentQueue<TDatagram> _incomingDatagrams = new ConcurrentQueue<TDatagram>();

        public void Enqueue(IEnumerable<TDatagram> Datagrams)
        {
            foreach (var datagram in Datagrams)
            {
                _incomingDatagrams.Enqueue(datagram);
            }
        }
        public IEnumerable<TDatagram> Read(TimeSpan Timeout = new TimeSpan(), bool ThrowExceptionOnTimeOut = false)
        {
            while (true)
            {
                TDatagram dtg = default(TDatagram);
                var ok = SpinWait.SpinUntil(() => _incomingDatagrams.TryDequeue(out dtg), Timeout);
                if (!ok) throw new TimeoutException("Превышено время ожидания дейтаграммы");
                yield return dtg;
            }
        }
    }
}