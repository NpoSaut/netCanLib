using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly AppiSetBaudRateBufferEncoder<TLineKey> _setBaudRateBufferEncoder;

        public AppiCanPortFactory(AppiSetBaudRateBufferEncoder<TLineKey> SetBaudRateBufferEncoder, AppiSendFramesBufferEncoder<TLineKey> SendFramesBufferEncoder)
        {
            _setBaudRateBufferEncoder = SetBaudRateBufferEncoder;
            _sendFramesBufferEncoder = SendFramesBufferEncoder;
        }

        public ICanPort ProduceCanPort(TLineKey Interface, IObservable<AppiLineStatus> LineStatusFlow, IObserver<UsbFrame> BufferSendPipe)
        {
            IObservable<AppiLineStatus> statusFlow = LineStatusFlow.Publish().RefCount();

            var options = new AppiCanPortOptions();
            options.RequestChangeBaudRate += (s, e) =>
                                             {
                                                 var message = new AppiSetBaudRateBuffer<TLineKey>(e.NewBaudRate, Interface);
                                                 BufferSendPipe.OnNext(new UsbFrame(_setBaudRateBufferEncoder.Encode(message)));
                                             };
            statusFlow.Select(status => status.BaudRate)
                      .Distinct()
                      .Subscribe(options.UpdateBitrate);

            var port = new AppiCanPort(statusFlow.SelectMany(line => line.Frames), options);

            port.TxOutput
                .Limit(statusFlow.Select(status => 30 - status.SendQueueSize))
                .Select(SubmitTransaction)
                .Select(x => new AppiSendFramesBuffer<TLineKey>(Interface, x.ToList()))
                .Select(m => _sendFramesBufferEncoder.Encode(m))
                .Select(data => new UsbFrame(data))
                .Subscribe(BufferSendPipe);

            return port;
        }

        private IEnumerable<CanFrame> SubmitTransaction(IEnumerable<AppiCanTransmitTransaction> Transactions)
        {
            foreach (AppiCanTransmitTransaction transaction in Transactions)
            {
                transaction.Commit();
                yield return transaction.Payload;
            }
        }
    }
}
