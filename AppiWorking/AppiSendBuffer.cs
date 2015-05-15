using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Communications.Appi.Buffers;
using Communications.Appi.Exceptions;
using Communications.Can;
using Buffer = System.Buffer;

namespace Communications.Appi
{
    abstract class AppiSendBuffer
    {
        protected const int FramesPerSendGroup = 20;

        protected AppiDev Device { get; set; }
        public AppiLine Line { get; set; }
        protected object Locker { get; set; }

        protected AppiSendBuffer(AppiDev Device, AppiLine Line)
        {
            this.Device = Device;
            this.Line = Line;
            Locker = new object();
        }

        protected IEnumerable<Byte[]> EncodeBuffers(IEnumerable<CanFrame> Frames)
        {
            var frameGroups = Frames
                .Select((f, i) => new { f, i })
                .GroupBy(fi => fi.i / FramesPerSendGroup, fi => fi.f)
                .Select(fg => fg.ToList());

            foreach (var fg in frameGroups)
            {
                Byte[] buff = new Byte[2048];
                Buffer.SetByte(buff, 0, 0x02);
                Buffer.SetByte(buff, 1, (byte)Line);
                //Buffer.SetByte(Buff, 2, SendMessageCounter);
                Buffer.SetByte(buff, 3, (byte)fg.Count);

                var messagesBuffer = fg.SelectMany(m => m.ToBufferBytes()).ToArray();
                Buffer.BlockCopy(messagesBuffer, 0, buff, 10, messagesBuffer.Length);

                yield return buff;
            }
        }

        public void SynchronizedSend(IList<CanFrame> Frames) { SynchronizedSend(Frames, TimeSpan.FromMilliseconds(-1)); }
        public abstract void SynchronizedSend(IList<CanFrame> Frames, TimeSpan Timeout);

        private bool _transfersAborted = false;
        public void AbortAllTransfers()
        {
            lock (Locker)
            {
                _transfersAborted = true;
                Monitor.PulseAll(Locker);
            }
        }

        protected void CheckAborted()
        {
            if (_transfersAborted) throw new TransferAbortedException();
        }
    }

    class AppiTimeoutSendBuffer : AppiSendBuffer
    {
        private DateTime _nextSendAviableAt;
        public AppiTimeoutSendBuffer(AppiDev Device, AppiLine Line) : base(Device, Line) {}

        public override void SynchronizedSend(IList<CanFrame> Frames, TimeSpan Timeout)
        {
            int sent = 0;
            foreach (var buffer in EncodeBuffers(Frames))
            {
                lock (Locker)
                {
                    SpinWait.SpinUntil(() => DateTime.Now >= _nextSendAviableAt);
                    CheckAborted();
                    Device.WriteBuffer(buffer);
                    int nowSent = Math.Min(FramesPerSendGroup, Frames.Count - sent);
                    _nextSendAviableAt = DateTime.Now.AddMilliseconds(nowSent*2);
                    sent += nowSent;
                }
            }
        }
    }

    class AppiFeedbackSendBuffer : AppiSendBuffer
    {
        public AppiFeedbackSendBuffer(AppiDev Device, AppiLine Line) : base(Device, Line)
        {
            Device.BufferRead += DeviceOnBufferRead;
        }

        private void DeviceOnBufferRead(object Sender, AppiBufferReadEventArgs AppiBufferReadEventArgs)
        {
            var messagesBuffer = AppiBufferReadEventArgs.Buffer as MessagesAppiBuffer222;
            if (messagesBuffer == null) return;
            PostCount(messagesBuffer.OutMessagesCount[Line]);
        }

        private void PostCount(int OutMessagesCount)
        {
            if (OutMessagesCount > 40) return;
            lock (Locker)
            {
                Monitor.Pulse(Locker);
            }
        }

        public override void SynchronizedSend(IList<CanFrame> Frames, TimeSpan Timeout)
        {
            foreach (var buffer in EncodeBuffers(Frames))
            {
                lock (Locker)
                {
                    CheckAborted();
                    var waitResult = Monitor.Wait(Locker, Timeout);
                    CheckAborted();
                    if (!waitResult) throw new TimeoutException("Превышено время ожидания готовности АППИ к приёму нового буфера сообщений");
                    Device.WriteBuffer(buffer);
                }
            }
        }
    }
}