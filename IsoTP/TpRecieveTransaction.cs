using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Communications.Can;
using Communications.Exceptions;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class TpReceiveTransaction : TpTransaction
    {
        /// <summary>
        /// Буфер приёма пакета
        /// </summary>
        private Byte[] Buff { get; set; }
        /// <summary>
        /// Указатель на место, в которое осуществляется приём пакета
        /// </summary>
        private int Pointer { get; set; }
        /// <summary>
        /// Ожидаемый последовательный номер пакета
        /// </summary>
        private int ExpectingConsIndex { get; set; }

        public TimeSpan SeparationTime { get; set; }
        public Byte BlockSize { get; set; }

        /// <summary>
        /// Возвращает полученные данные. Блокирует текущий поток, пока данные не будут получены.
        /// </summary>
        public Byte[] Data
        {
            get
            {
                Wait();
                return Buff;
            }
        }

        public TpReceiveTransaction(ICanSocket Socket, int TransmitDescriptor, int AcknowledgmentDescriptor)
            : base(Socket, TransmitDescriptor, AcknowledgmentDescriptor)
        {
            this.SeparationTime = TimeSpan.Zero;
            this.BlockSize = 20;
        }

        public Byte[] Receive()
        {
            if (this.Status != TpTransactionStatus.Ready) throw new IsoTpTransactionReuseException(this);
            this.Status = TpTransactionStatus.Active;

            bool transactionStarted = false;

            try
            {
                // ReSharper disable PossibleMultipleEnumeration
                // Инициализируем чтение с заданным таймаутом,
                // при истечении таймаута - выбрасываем ошибку.
                var framesStream = Socket.ReadWithTimeout(flow => flow.Where(f => f.Descriptor == TransmitDescriptor), Timeout, true);

                try
                {
                    // Ждём первого кадра передачи
                    FirstFrame firstFrame = null;
                    foreach (var f in framesStream)
                    {
                        var ft = f.GetIsoTpFrameType();
                        if (ft == IsoTpFrameType.First)
                        {
                            firstFrame = (FirstFrame)f;
                            break;
                        }
                        if (ft == IsoTpFrameType.Single)
                        {
                            Buff = ((SingleFrame)f).Data;
                            Status = TpTransactionStatus.Done;
                            return Data;
                        }
                    }
                    transactionStarted = true;

                    // После того, как поймали первый кадр - подготавливаем буфер
                    Buff = new Byte[firstFrame.PacketSize];
                    Buffer.BlockCopy(firstFrame.Data, 0, Buff, 0, firstFrame.Data.Length);
                    Pointer += firstFrame.Data.Length;

                    ExpectingConsIndex = 1;

                    // Начинаем приём данных
                    while (Pointer < Buff.Length)
                    {
                        SendFlowControl();          // Сообщаем о готовности
                        ReadBlock(framesStream);    // Читаем следующий блок
                    }
                }
                catch
                {
                    this.Status = TpTransactionStatus.Error;
                    // Если в процессе передачи возникла ошибка, отправляем отмену
                    if (transactionStarted)
                        Socket.Send(FlowControlFrame.AbortFrame.GetCanFrame(AcknowledgmentDescriptor));
                    throw;      // и пробрасываем ошибку дальше по стеку
                }
                // ReSharper restore PossibleMultipleEnumeration
            }
            catch (SocketTimeoutException timeoutException)
            {
                throw new IsoTpReceiveTimeoutException(timeoutException);
            }

            this.Status = TpTransactionStatus.Done;
            return Buff;
        }

        private void ReadBlock(IEnumerable<CanFrame> FromStream)
        {
            var Consequence = FromStream
                    .Where(f => f.GetIsoTpFrameType() == IsoTpFrameType.Consecutive)
                    .Select(f => (ConsecutiveFrame)f)
                    .Take(BlockSize);

            foreach (var cf in Consequence)
            {
                if (cf.Index != ExpectingConsIndex) throw new IsoTpSequenceException(ExpectingConsIndex, cf.Index);

                int DataLength = Math.Min(cf.Data.Length, Buff.Length - Pointer);
                Buffer.BlockCopy(cf.Data, 0, Buff, Pointer, DataLength);
                Pointer += cf.Data.Length;
                ExpectingConsIndex = (ExpectingConsIndex + 1) & 0x0f;

                if (Pointer >= Buff.Length) break;
            }
        }
        private void SendFlowControl()
        {
            Socket.Send(GenerateFlowControl().GetCanFrame(AcknowledgmentDescriptor));
        }
        private FlowControlFrame GenerateFlowControl()
        {
            return new FlowControlFrame(FlowControlFlag.ClearToSend, BlockSize, SeparationTime);
        }
    }
}
