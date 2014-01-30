using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Communications.Exceptions;

namespace Communications.Sockets
{
    /// <summary>
    /// Абстракция сокета, буферизирующего все входящие дейтаграммы до момента их прочтения
    /// </summary>
    /// <remarks>
    /// Разделяет процессы накладывания сообщений в себя и изъятия. Не препятствует помещению дейтаграмм в буфер, позволяя сделать это как можно быстрее.
    /// При изъятии сообщений выдаёт сообщение из буфера либо блокирует вызов до появление в буфере сообщений.
    /// </remarks>
    public abstract class BufferedSocketBase<TDatagram> : SocketBase<TDatagram>
    {
        private readonly IDatagramBuffer<TDatagram> _buffer;

        protected BufferedSocketBase(string Name) : this(Name, new ConcurrentDatagramBuffer<TDatagram>()) { }
        protected BufferedSocketBase(string Name, IDatagramBuffer<TDatagram> Buffer) : base(Name) { _buffer = Buffer; }

        /// <summary>Проверяет, нужно ли помещать дейтаграмму в буфер. При необходимости можно заменить, чтобы не вызывать переполнение буфера лишними дейтаграммамаи</summary>
        protected virtual bool CheckDatagramBeforeEnqueue(TDatagram Datagram) { return true; }

        /// <summary>Добавляет датаграммы в очередь на обработку</summary>
        /// <param name="Datagrams">Полученные датаграммы</param>
        public override void ProcessReceivedDatagrams(IEnumerable<TDatagram> Datagrams) { _buffer.Enqueue(Datagrams.Where(CheckDatagramBeforeEnqueue)); }

        /// <summary>
        /// Выполняет блокирующее считывание дейтаграммы из входящего потока до тех пор, пока время между соседними дейтаграммами не превысит указанный таймаут
        /// </summary>
        public override IEnumerable<TDatagram> Receive(TimeSpan Timeout = default(TimeSpan), bool ThrowExceptionOnTimeout = false)
        {
            return _buffer.Read(Timeout, ThrowExceptionOnTimeout);
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
        public IEnumerable<TDatagram> Read(TimeSpan Timeout = default(TimeSpan), bool ThrowExceptionOnTimeOut = false)
        {
            if (Timeout == TimeSpan.Zero) Timeout = TimeSpan.FromMilliseconds(-1);
            while (true)
            {
                TDatagram dtg = default(TDatagram);
                var ok = SpinWait.SpinUntil(() => _incomingDatagrams.TryDequeue(out dtg), Timeout);
                if (ok) yield return dtg;
                else if (ThrowExceptionOnTimeOut) throw new SocketReadTimeoutException("Превышено время ожидания дейтаграммы");
                else yield break;
            }
        }
    }
}