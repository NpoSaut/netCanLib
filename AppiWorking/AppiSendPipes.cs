using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Communications.Appi.Buffers;
using Communications.Appi.Exceptions;
using Communications.Can;
using Communications.Exceptions;

namespace Communications.Appi
{
    public abstract class AppiSendPipe : ISendPipe<CanFrame>
    {
        protected const int FramesPerSendGroup = 20;

        protected object Locker { get; set; }

        protected AppiSendPipe()
        {
            Locker = new object();
        }

        protected IEnumerable<Byte[]> EncodeBuffers(IEnumerable<CanFrame> Frames)
        {
        }

        /// <summary>
        /// Отправляет фреймы, обеспечивая защиту от переполнения исходящего буфера АППИ
        /// </summary>
        /// <param name="Frames">Фреймы для отправки</param>
        public void Send(IList<CanFrame> Frames) { Send(Frames, TimeSpan.FromMilliseconds(-1)); }
        /// <summary>
        /// Отправляет фреймы, обеспечивая защиту от переполнения исходящего буфера АППИ и отслеживая таймаут операции
        /// </summary>
        /// <param name="Frames">Фреймы для отправки</param>
        /// <param name="Timeout">Таймаут операции</param>
        /// <exception cref="SocketSendTimeoutException">Выбрасывается при превышении таймаута ожидания готовности исходящего буфера</exception>
        public abstract void Send(IList<CanFrame> Frames, TimeSpan Timeout);

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

    class AppiTimeoutSendPipe : AppiSendPipe
    {
        private DateTime _nextSendAviableAt;
        public AppiTimeoutSendPipe(AppiDev Device, AppiLine Line) : base(Device, Line) {}

        public override void Send(IList<CanFrame> Frames, TimeSpan Timeout)
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

    class AppiFeedbackSendPipe : AppiSendPipe
    {
        private void DeviceOnBufferRead(object Sender, AppiBufferReadEventArgs AppiBufferReadEventArgs)
        {
            var messagesBuffer = AppiBufferReadEventArgs.Buffer as MessagesReadAppiBuffer;
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

        public override void Send(IList<CanFrame> Frames, TimeSpan Timeout)
        {
            foreach (var buffer in EncodeBuffers(Frames))
            {
                lock (Locker)
                {
                    CheckAborted();
                    var waitResult = Monitor.Wait(Locker, Timeout);
                    CheckAborted();
                    if (!waitResult) throw new SocketSendTimeoutException("Превышено время ожидания готовности АППИ к приёму нового буфера сообщений");
                    Device.WriteBuffer(buffer);
                }
            }
        }
    }
}