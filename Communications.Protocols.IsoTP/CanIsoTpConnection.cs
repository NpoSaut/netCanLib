using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Threading;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class CanIsoTpConnection : IsoTpConnectionBase, IDisposable
    {
        private readonly ConcurrentQueue<IsoTpFrame> _framesQueue = new ConcurrentQueue<IsoTpFrame>();
        private readonly IDisposable _rxConnection;
        private readonly IObserver<CanFrame> _tx;

        public CanIsoTpConnection(ICanPort Port,
                                  ushort TransmitDescriptor, ushort ReceiveDescriptor,
                                  int ReceiveBlockSize = 128, int SeparationTimeMs = 0)
            : this(Port.Rx, Port.Tx, TransmitDescriptor, ReceiveDescriptor, ReceiveBlockSize, SeparationTimeMs) { }

        public CanIsoTpConnection(IObservable<CanFrame> Rx, IObserver<CanFrame> Tx,
                                  ushort TransmitDescriptor, ushort ReceiveDescriptor,
                                  int ReceiveBlockSize = 128, int SeparationTimeMs = 0)
            : base(ReceiveBlockSize, SeparationTimeMs)
        {
            this.ReceiveDescriptor = ReceiveDescriptor;
            this.TransmitDescriptor = TransmitDescriptor;

            _tx = Tx;

            _rxConnection = Rx
                .Where(f => f.Descriptor == ReceiveDescriptor)
                .Select(f => IsoTpFrame.ParsePacket(f.Data))
                .Subscribe(f => _framesQueue.Enqueue(f));
        }

        private ushort TransmitDescriptor { get; set; }
        private ushort ReceiveDescriptor { get; set; }

        public override int SubframeLength
        {
            get { return 8; }
        }

        public void Dispose() { _rxConnection.Dispose(); }

        public override IsoTpFrame ReadNextFrame(TimeSpan Timeout)
        {
            IsoTpFrame frame = null;
            if (!SpinWait.SpinUntil(() => _framesQueue.TryDequeue(out frame), Timeout))
                throw new TimeoutException();
            return frame;
        }

        public override void SendFrame(IsoTpFrame Frame) { _tx.OnNext(Frame.GetCanFrame(TransmitDescriptor)); }
    }
}
