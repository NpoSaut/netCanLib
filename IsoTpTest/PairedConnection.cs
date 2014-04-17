using System;
using System.Collections.Concurrent;
using System.Threading;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Frames;

namespace IsoTpTest
{
    internal class PairedConnection : IsoTpConnectionBase
    {
        private static readonly PairedConnectionBuilder StaticBuilder = new PairedConnectionBuilder();

        private PairedConnection(ConcurrentQueue<IsoTpFrame> InputQueue, ConcurrentQueue<IsoTpFrame> OutputQueue)
        {
            this.InputQueue = InputQueue;
            this.OutputQueue = OutputQueue;
        }

        public override int SubframeLength
        {
            get { return 8; }
        }

        private ConcurrentQueue<IsoTpFrame> InputQueue { get; set; }
        private ConcurrentQueue<IsoTpFrame> OutputQueue { get; set; }

        public static PairedConnectionBuilder Builder
        {
            get { return StaticBuilder; }
        }

        public override void SendFrame(IsoTpFrame Frame) { OutputQueue.Enqueue(Frame); }

        public override IsoTpFrame ReadNextFrame(TimeSpan Timeout)
        {
            IsoTpFrame frame = null;
            SpinWait.SpinUntil(() => InputQueue.TryDequeue(out frame));
            return frame;
        }

        public class PairedConnectionBuilder
        {
            public PairedConnection[] Build()
            {
                var queues =
                    new[]
                    {
                        new ConcurrentQueue<IsoTpFrame>(),
                        new ConcurrentQueue<IsoTpFrame>()
                    };
                var connections =
                    new[]
                    {
                        new PairedConnection(queues[0], queues[1]),
                        new PairedConnection(queues[1], queues[0])
                    };
                return connections;
            }
        }
    }
}
