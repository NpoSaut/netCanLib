using System;
using System.Collections.Generic;
using System.IO;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace IsoTpTest.StatesTests
{
    public abstract class IsoTpStateTest
    {
        protected readonly Random Rnd = new Random();

        protected Byte[] GetRandomBytes(int Count)
        {
            var res = new byte[Count];
            Rnd.NextBytes(res);
            return res;
        }

        protected class TestIsoTpConnection : IsoTpConnectionBase
        {
            private readonly int _subframeLength;

            public TestIsoTpConnection(int SubframeLength = 8, int BlockLength = 128, int SeparationTimeMs = 0)
                : base(BlockLength, SeparationTimeMs)
            {
                SentFrames = new Queue<IsoTpFrame>();
                IncomingQueue = new Queue<IsoTpFrame>();
                _subframeLength = SubframeLength;
            }

            public override int SubframeLength
            {
                get { return _subframeLength; }
            }

            public Queue<IsoTpFrame> IncomingQueue { get; private set; }
            public Queue<IsoTpFrame> SentFrames { get; private set; }

            public override void SendFrame(IsoTpFrame Frame) { SentFrames.Enqueue(Frame); }

            public override IsoTpFrame ReadNextFrame(TimeSpan Timeout)
            {
                if (IncomingQueue.Count > 0)
                    return IncomingQueue.Dequeue();
                else
                    throw new IsoTpTimeoutException();
            }

            public TpTransaction FinishedTransaction { get; private set; }

            public override void OnTransactionReady(TpTransaction Transaction)
            {
                FinishedTransaction = Transaction;
                base.OnTransactionReady(Transaction);
            }
        }
    }
}