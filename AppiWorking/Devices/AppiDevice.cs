using System;
using System.Collections.Generic;
using Communications.Appi.Buffers;
using Communications.Appi.Decoders;
using Buffer = Communications.Appi.Buffers.Buffer;

namespace Communications.Appi.Devices
{
    public abstract class AppiDevice<TLineKey>
        where TLineKey : struct, IConvertible
    {
        private readonly IAppiBufferDecoder _decoder;
        private readonly IUsbDevice _usbDevice;
        private int _lastReceivedBuffer = -1;

        public AppiDevice(IUsbDevice UsbDevice, IAppiBufferDecoder Decoder)
        {
            _usbDevice = UsbDevice;
            _decoder = Decoder;
        }

        public IDictionary<TLineKey, AppiCanPort> CanPorts { get; protected set; }

        private void Do()
        {
            // Чтение и расшифровка буфера
            byte[] buff = _usbDevice.ReadBuffer();
            Buffer buffer = _decoder.DecodeBuffer(buff);

            // Проверка сквозного номера
            if (buffer.SequentialNumber == _lastReceivedBuffer)
                return;
            _lastReceivedBuffer = buffer.SequentialNumber;

            // Обработка расшифрованного буфера
            ProcessAppiBuffer(buffer);
        }

        private void ProcessAppiBuffer(Buffer AppiBuffer)
        {
            if (AppiBuffer is MessagesBuffer<TLineKey>)
                ProcessMessagesAppiBuffer((MessagesBuffer<TLineKey>)AppiBuffer);
        }

        private void ProcessMessagesAppiBuffer(MessagesBuffer<TLineKey> AppiBuffer)
        {
            foreach (var lineStatus in AppiBuffer.LineStatuses)
                CanPorts[lineStatus.Key].UpdateStatus(lineStatus.Value);
        }
    }
}
