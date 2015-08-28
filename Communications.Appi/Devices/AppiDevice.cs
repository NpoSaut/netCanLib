using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Appi.Buffers;
using Communications.Appi.Decoders;
using Communications.Appi.Encoders;
using Communications.Appi.Ports;
using Communications.Can;
using Communications.PortHelpers;
using Communications.Usb;
using Buffer = Communications.Appi.Buffers.Buffer;

namespace Communications.Appi.Devices
{
    public abstract class AppiDevice<TLineKey> : IDisposable where TLineKey : struct, IConvertible
    {
        private readonly IDisposable _buffersStreamConnection;
        private readonly IAppiBufferDecoder _decoder;
        private readonly IUsbDevice _usbDevice;

        public AppiDevice(IUsbDevice UsbDevice, IEnumerable<TLineKey> LineKeys, IAppiBufferDecoder Decoder,
                          AppiSendFramesBufferEncoder<TLineKey> SendFramesBufferEncoder)
        {
            _decoder = Decoder;
            _usbDevice = UsbDevice;

            IConnectableObservable<Buffer> buffersStream = _usbDevice.Rx
                                                                     .WaitForTransactionCompleated()
                                                                     .Select(frame => _decoder.DecodeBuffer(frame.Data))
                                                                     .Publish();
            _buffersStreamConnection = buffersStream.Connect();

            var fac = new AppiCanPortFactory<TLineKey>(SendFramesBufferEncoder);
            CanPorts =
                LineKeys.ToDictionary(key => key,
                                      key => fac.ProduceCanPort(key,
                                                                buffersStream.OfType<MessagesBuffer<TLineKey>>().Select(buffer => buffer.LineStatuses[key]),
                                                                Observer.Create<UsbFrame>(f => _usbDevice.BeginSend(f))));
        }

        public IDictionary<TLineKey, ICanPort> CanPorts { get; private set; }

        public void Dispose()
        {
            _buffersStreamConnection.Dispose();
            _usbDevice.Dispose();
        }
    }
}
