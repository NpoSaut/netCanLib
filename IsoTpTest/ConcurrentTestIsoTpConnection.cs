using System;
using System.Collections.Concurrent;
using System.Threading;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Frames;

namespace IsoTpTest
{
    public class ConcurrentTestIsoTpConnection : IsoTpConnectionBase
    {
        public ConcurrentTestIsoTpConnection(int BlockLength, int SeparationTimeMs)
            : base(BlockLength, SeparationTimeMs)
        {
            OutputQueue = new ConcurrentQueue<IsoTpFrame>();
            InputQueue = new ConcurrentQueue<IsoTpFrame>();
        }

        public override int SubframeLength
        {
            get { return 8; }
        }

        public ConcurrentQueue<IsoTpFrame> OutputQueue { get; private set; }
        public ConcurrentQueue<IsoTpFrame> InputQueue { get; private set; }

        public override void SendFrame(IsoTpFrame Frame) { OutputQueue.Enqueue(Frame); }

        public override IsoTpFrame ReadNextFrame(TimeSpan Timeout)
        {
            IsoTpFrame frame = null;
            SpinWait.SpinUntil(() => InputQueue.TryDequeue(out frame), Timeout);
            return frame;
        }

        public TpTransaction FinishedTransaction { get; private set; }

        public override void OnTransactionReady(TpTransaction Transaction)
        {
            FinishedTransaction = Transaction;
            base.OnTransactionReady(Transaction);
        }
    }
}