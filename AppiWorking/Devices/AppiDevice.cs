using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Communications.Appi.Buffers;
using Communications.Appi.Decoders;
using Communications.Appi.Encoders;
using Communications.Appi.Ports;
using Communications.Can;
using Communications.Usb;
using Buffer = Communications.Appi.Buffers.Buffer;

namespace Communications.Appi.Devices
{
    public abstract class AppiDevice<TLineKey> : IDisposable where TLineKey : struct, IConvertible
    {
        private readonly IAppiBufferDecoder _decoder;
        private readonly AppiSendFramesBufferEncoder<TLineKey> _sendFramesBufferEncoder;
        private readonly IUsbDevice _usbDevice;

        public AppiDevice(IUsbDevice UsbDevice, IEnumerable<TLineKey> LineKeys, IAppiBufferDecoder Decoder,
                          AppiSendFramesBufferEncoder<TLineKey> SendFramesBufferEncoder)
        {
            _decoder = Decoder;
            _sendFramesBufferEncoder = SendFramesBufferEncoder;
            _usbDevice = UsbDevice;

            IObservable<Buffer> buffersStream = _usbDevice.Rx.Select(frame => _decoder.DecodeBuffer(frame.Data));

            IObservable<MessagesBuffer<TLineKey>> messageBuffersStream = buffersStream.OfType<MessagesBuffer<TLineKey>>();

            var fac = new AppiCanPortFactory();
            CanPorts =
                LineKeys.ToDictionary(key => key,
                                      key =>
                                      {
                                          AppiCanPort port = fac.produceCanPort(messageBuffersStream.Select(buffer => buffer.LineStatuses[key]));
                                          port.TxOutput
                                              .Select(x => new AppiSendFramesBuffer<TLineKey>(key, x))
                                              .Select(m => _sendFramesBufferEncoder.Encode(m))
                                              .Select(data => new UsbFrame(data))
                                              .Subscribe(_usbDevice.Tx);
                                          return (ICanPort)port;
                                      });
        }

        public IDictionary<TLineKey, ICanPort> CanPorts { get; private set; }

        //        private void Do()
        //        {
        //            // Чтение и расшифровка буфера
        //            byte[] buff = _usbDevice.ReadBuffer();
        //            Buffer buffer = _decoder.DecodeBuffer(buff);
        //
        //            // Проверка сквозного номера
        //            if (buffer.SequentialNumber == _lastReceivedBuffer)
        //                return;
        //            _lastReceivedBuffer = buffer.SequentialNumber;
        //
        //            // Обработка расшифрованного буфера
        //            ProcessAppiBuffer(buffer);
        //        }

        public void Dispose() { _usbDevice.Dispose(); }
    }
}
