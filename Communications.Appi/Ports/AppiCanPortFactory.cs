using System;
using System.Reactive.Linq;
using Communications.Appi.Buffers;
using Communications.Appi.Encoders;
using Communications.Can;
using Communications.Usb;

namespace Communications.Appi.Ports
{
    public class AppiCanPortFactory<TLineKey>
    {
        private readonly AppiSendFramesBufferEncoder<TLineKey> _sendFramesBufferEncoder;
        public AppiCanPortFactory(AppiSendFramesBufferEncoder<TLineKey> SendFramesBufferEncoder) { _sendFramesBufferEncoder = SendFramesBufferEncoder; }

        public ICanPort ProduceCanPort(TLineKey Interface, IObservable<AppiLineStatus> LineStatusFlow, IObserver<UsbFrame> BufferSendPipe)
        {
            IObservable<AppiLineStatus> statusFlow = LineStatusFlow.Publish().RefCount();

            var port = new AppiCanPort(statusFlow.SelectMany(line => line.Frames));

            port.TxOutput
                .Limit(statusFlow.Select(status => 30 - status.SendQueueSize))
                .Select(x => new AppiSendFramesBuffer<TLineKey>(Interface, x))
                .Select(m => _sendFramesBufferEncoder.Encode(m))
                .Select(data => new UsbFrame(data))
                .Subscribe(BufferSendPipe);

            return port;
        }
    }
}
