using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Communications.Appi.Buffers;
using Communications.Appi.Exceptions;
using Communications.Can;

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
            var FrameGroups = Frames
                .Select((f, i) => new { f, i })
                .GroupBy(fi => fi.i / FramesPerSendGroup, fi => fi.f)
                .Select(fg => fg.ToList());

            foreach (var fg in FrameGroups)
            {
                Byte[] Buff = new Byte[2048];
                Buffer.SetByte(Buff, 0, 0x02);
                Buffer.SetByte(Buff, 1, (byte)Line);
                //Buffer.SetByte(Buff, 2, SendMessageCounter);
                Buffer.SetByte(Buff, 3, (byte)fg.Count);

                var messagesBuffer = fg.SelectMany(m => m.ToBufferBytes()).ToArray();
                Buffer.BlockCopy(messagesBuffer, 0, Buff, 10, messagesBuffer.Length);

                yield return Buff;
            }
        }

        public abstract void SyncronizedSend(IList<CanFrame> Frames);

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

        public override void SyncronizedSend(IList<CanFrame> Frames)
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
            var messagesBuffer = AppiBufferReadEventArgs.Buffer as MessagesReadAppiBuffer;
            if (messagesBuffer == null) return;
            PostCount(messagesBuffer.OutMessagesCount[Line]);
        }

        public void PostCount(int OutMessagesCount)
        {
            if (OutMessagesCount > 40) return;
            lock (Locker)
            {
                Monitor.Pulse(Locker);
            }
        }

        public override void SyncronizedSend(IList<CanFrame> Frames)
        {
            foreach (var buffer in EncodeBuffers(Frames))
            {
                lock (Locker)
                {
                    CheckAborted();
                    Monitor.Wait(Locker);
                    CheckAborted();
                    Device.WriteBuffer(buffer);
                }
            }
        }
    }
}