using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class IsoTpConnection : IIsoTpConnection
    {
        private static int _counter;
        private readonly int _index;

        private readonly IsoReceiveObservable _receiver;

        private readonly IDisposable _rxConnection;
        private readonly IsoTpSendObserver _sender;

        public IsoTpConnection(IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx, byte ReceiveBlockSize, TimeSpan SeparationTime, TimeSpan Timeout,
                               int FrameLayerCapacity)
        {
            Options = new DataPortOptions<IsoTpPacket>(4095);
            _index = ++_counter;

            IConnectableObservable<IsoTpFrame> sharedRx = Rx.Publish();

            _receiver = new IsoReceiveObservable(sharedRx, Tx, ReceiveBlockSize, SeparationTime, Timeout);
            _sender = new IsoTpSendObserver(sharedRx, Tx, Timeout, FrameLayerCapacity);

            sharedRx.Subscribe(f => Debug.Print("ISO-TP {1}:          <-- {0}", f, _index));

            _rxConnection = sharedRx.Connect();
        }

        public IObservable<IsoTpPacket> Rx
        {
            get { return _receiver; }
        }

        public IObserver<IsoTpPacket> Tx
        {
            get { return _sender; }
        }

        /// <summary>Опции порта</summary>
        public DataPortOptions<IsoTpPacket> Options { get; private set; }

        public void Dispose()
        {
            _rxConnection.Dispose();
            _sender.Dispose();
        }
    }
}
