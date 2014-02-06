using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Communications.Appi.Buffers;
using Communications.Can;
using Communications.Piping;
using Communications.Usb;

namespace Communications.Appi
{
    public class AppiEngine : IDisposable
    {
        public IUsbBulkSocket UsbSocket { get; private set; }

        private readonly Thread _readingThread;

        public AppiEngine(IUsbBulkSocket UsbSocket)
        {
            this.UsbSocket = UsbSocket;
            _readingThread = new Thread(ReadLoop)
                            {
                                Name = "Поток прослушивания АППИ"
                            };

            ReceivePipes = new Dictionary<AppiLine, RedirectReceivePipe<CanFrame>>
                            {
                               { AppiLine.Can1, new RedirectReceivePipe<CanFrame>() },
                               { AppiLine.Can2, new RedirectReceivePipe<CanFrame>() }
                           };
        }

        private void ReadLoop()
        {
            foreach (var bulk in UsbSocket.Receive())
            {
                var buffer = AppiBufferBase.Decode(bulk.Data);
                if (buffer != null) ProcessBuffer(buffer);
            }
        }

        private int _lastBufferId = -1;

        private void ProcessBuffer(AppiBufferBase buffer)
        {
            if (buffer.SequentNumber == _lastBufferId) return;
            _lastBufferId = buffer.SequentNumber;

            if (buffer is MessagesReadAppiBuffer) ProcessMessagesBuffer((MessagesReadAppiBuffer)buffer);
            if (buffer is VersionReadAppiBuffer) ParseVersionBuffer((VersionReadAppiBuffer)buffer);
        }

        #region Version

        public Version AppiVersion { get; private set; }
        private readonly object _appiVersionLocker = new object();

        private void ParseVersionBuffer(VersionReadAppiBuffer Buffer)
        {
            lock (_appiVersionLocker)
            {
                AppiVersion = Buffer.AppiVersion;
                Monitor.PulseAll(_appiVersionLocker);
            }
        }

        #endregion

        public IDictionary<AppiLine, RedirectReceivePipe<CanFrame>> ReceivePipes { get; private set; };

        private void ProcessMessagesBuffer(MessagesReadAppiBuffer buffer)
        {
            foreach (var canMessagesBuffer in buffer.CanMessages)
                ReceivePipes[canMessagesBuffer.Key].OnDatagramsReceived(new DatagramsReceivedEventArgs<CanFrame>(canMessagesBuffer.Value));
        }

        public void Dispose()
        {
            _readingThread.Abort();
        }
    }
}