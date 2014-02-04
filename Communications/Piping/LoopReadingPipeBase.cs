using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Communications.Piping
{
    public abstract class LoopReadingPipeBase<TDatagram> : IReceivePipe<TDatagram>, IDisposable
    {
        private readonly Thread _readingThread;

        protected LoopReadingPipeBase()
        {
            _readingThread = new Thread(new ThreadStart(ReadingLoop));
        }

        protected void Start()
        {
            _readingThread.Start();
        }

        private void ReadingLoop()
        {
            IList<TDatagram> datagrams = new TDatagram[0];
            while (true)
            {
                SpinWait.SpinUntil(() => (datagrams = ReadDatagrams()).Any());
                OnDatagramsReceived(new DatagramsReceivedEventArgs<TDatagram>(datagrams));
            }
        }

        protected abstract IList<TDatagram> ReadDatagrams();

        public event EventHandler<DatagramsReceivedEventArgs<TDatagram>> DatagramsReceived;
        protected virtual void OnDatagramsReceived(DatagramsReceivedEventArgs<TDatagram> E)
        {
            var handler = DatagramsReceived;
            if (handler != null) handler(this, E);
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public virtual void Dispose() { _readingThread.Abort(); }
    }
}