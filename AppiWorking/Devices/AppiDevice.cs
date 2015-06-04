using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Communications.Appi.Buffers;
using Communications.Appi.Decoders;
using Communications.Appi.Ports;
using Communications.Can;
using Communications.Usb;
using Buffer = Communications.Appi.Buffers.Buffer;

namespace Communications.Appi.Devices
{
    public abstract class AppiDevice<TLineKey>
        where TLineKey : struct, IConvertible
    {
        private readonly IAppiBufferDecoder _decoder;
        private readonly IUsbDevice _usbDevice;
        private int _lastReceivedBuffer = -1;

        public AppiDevice(IUsbDevice UsbDevice, IAppiBufferDecoder Decoder, ICollection<TLineKey> LineKeys)
        {
            _decoder = Decoder;
            _usbDevice = UsbDevice;

            IObservable<Buffer> buffersStream = _usbDevice.Rx.Select(frame => _decoder.DecodeBuffer(frame.Data));

            IObservable<MessagesBuffer<TLineKey>> messageBuffersStream = buffersStream.OfType<MessagesBuffer<TLineKey>>();

            var fac = new AppiCanPortFactory();
            CanPorts =
                LineKeys.ToDictionary(key => key,
                                      key => (ICanPort)fac.produceCanPort(messageBuffersStream.Select(buffer => buffer.LineStatuses[key])));
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
    }
}
