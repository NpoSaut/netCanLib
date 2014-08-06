using System.Collections.Generic;
using System.Threading;
using Communications.Piping.Packages;

namespace Communications.Piping
{
    public abstract class PackageFeedbackSendingPipeBase<TDatagram> : ISendPipe<TDatagram>
    {
        private readonly object _sendLocker = new object();

        /// <summary>Передаёт дейтаграммы на низлежащий уровень для его отправки</summary>
        public void Send(IList<TDatagram> Frames)
        {
            IOutgoingPack<TDatagram> package = Enpack(Frames);
            lock (_sendLocker)
            {
                PushToSend(Frames);
            }
            SpinWait.SpinUntil(() => package.IsDone);
        }

        protected abstract IOutgoingPack<TDatagram> Enpack(IList<TDatagram> Datagrams);
        protected abstract void PushToSend(IList<TDatagram> Datagrams);
    }
}
