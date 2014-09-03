using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Communications.Appi.Buffers;
using Communications.Can;
using Communications.Piping;
using Communications.Usb;

namespace Communications.Appi
{
    internal class AppiEngine : IDisposable
    {
        private delegate void AppiBufferHandler(AppiEngine Engine, AppiBuffer Message);

        private static readonly Dictionary<Type, AppiBufferHandler> BufferHandlers =
            new Dictionary<Type, AppiBufferHandler>
            {
                { typeof(MessagesReadAppiBuffer), ProcessMessagesBuffer }
            };

        public AppiBufferSocket AppiSocket { get; private set; }

        private readonly Thread _readingThread;

        public AppiEngine(AppiBufferSocket AppiSocket)
        {
            this.AppiSocket = AppiSocket;
            _readingThread = new Thread(ReadLoop) { Name = "Поток прослушивания АППИ" };

            ReceivePipes = new Dictionary<AppiLine, RedirectReceivePipe<CanFrame>>
                            {
                               { AppiLine.Can1, new RedirectReceivePipe<CanFrame>() },
                               { AppiLine.Can2, new RedirectReceivePipe<CanFrame>() }
                           };

            SendPipes = new Dictionary<AppiLine, AppiSendPipe>
                        {
                            { AppiLine.Can1, new AppiFeedbackSendPipe() },
                            { AppiLine.Can2, new AppiFeedbackSendPipe() }
                        };
        }

        private int _lastBufferId = -1;

        /// <summary>Основная петля чтения из сокета АППИ</summary>
        private void ReadLoop()
        {
            foreach (AppiBuffer buffer in AppiSocket.Receive().Where(buffer => buffer != null))
            {
                if (buffer.SequentNumber == _lastBufferId) return;
                _lastBufferId = buffer.SequentNumber;

                Type bufferType = buffer.GetType();
                if (BufferHandlers.ContainsKey(bufferType))
                    BufferHandlers[bufferType](this, buffer);
            }
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

        public IDictionary<AppiLine, RedirectReceivePipe<CanFrame>> ReceivePipes { get; private set; }
        public IDictionary<AppiLine, AppiSendPipe> SendPipes { get; private set; }

        private static void ProcessMessagesBuffer(AppiEngine Engine, AppiBuffer Message)
        {
            var messagesBuffer = (MessagesReadAppiBuffer)Message;
            foreach (var canMessagesBuffer in messagesBuffer.CanMessages)
                Engine.ReceivePipes[canMessagesBuffer.Key].OnDatagramsReceived(new DatagramsReceivedEventArgs<CanFrame>(canMessagesBuffer.Value));
        }

        private void DirectSendFrames(IEnumerable<CanFrame> Frames, AppiLine Line)
        {
            UsbSocket.Send(EncodeFrames(Frames, Line).Select(b => new UsbBulk(b)));
        }

        private byte _sendMessageCounter = 0;
        private IEnumerable<Byte[]> EncodeFrames(IEnumerable<CanFrame> Frames, AppiLine Line)
        {
            const int framesPerSendGroup = 40;
            var frameGroups = Frames
                .Select((f, i) => new { f, i })
                .GroupBy(fi => fi.i / framesPerSendGroup, fi => fi.f)
                .Select(fg => fg.ToList());

            foreach (var fg in frameGroups)
            {
                Byte[] buff = new Byte[2048];
                Buffer.SetByte(buff, 0, 0x02);
                Buffer.SetByte(buff, 1, (byte)Line);
                Buffer.SetByte(buff, 2, ++_sendMessageCounter);
                Buffer.SetByte(buff, 3, (byte)fg.Count);

                var messagesBuffer = fg.SelectMany(m => m.ToBufferBytes()).ToArray();
                Buffer.BlockCopy(messagesBuffer, 0, buff, 10, messagesBuffer.Length);

                yield return buff;
            }
        }

        public void Dispose()
        {
            _readingThread.Abort();
        }
    }
}