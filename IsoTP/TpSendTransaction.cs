using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Communications.Can;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    public class TpSendTransaction : TpTransaction
    {
        private Byte[] Buff { get; set; }
        private int Pointer { get; set; }

        public int BlockSize { get; private set; }
        public TimeSpan SeparationTime { get; set; }

        public TpSendTransaction(ICanSocket Socket, int TransmitDescriptor, int AcknowledgmentDescriptor)
            : base(Socket, TransmitDescriptor, AcknowledgmentDescriptor)
        { }

        public void Send(TpPacket Packet)
        {
            if (this.Status != TpTransactionStatus.Ready) throw new IsoTpTransactionReuseException(this);
            this.Status = TpTransactionStatus.Active;
            this.Buff = Packet.Data;

            if (Buff.Length <= Frames.SingleFrame.DataCapacity) SendSingle();
            else SendFlow();

            this.Status = TpTransactionStatus.Done;
        }

        private void SendFlow()
        {
            var AckStream = Socket.Receive(Timeout, true).Where(f => f.Descriptor == AcknowledgmentDescriptor);

            try
            {
                Socket.Send(GetFirstFrame().GetCanFrame(TransmitDescriptor));

                // Берём очередь для отправки
                var PushingCanFrames = GetConsFrames().Select(cf => cf.GetCanFrame(TransmitDescriptor));

                // Повторяем, пока не отослали всю очередь
                while (Pointer < Buff.Length)
                {
                    // Дожидаемся FlowControl фрейма
                    while (ProcessFlowControl(AckStream) != FlowControlFlag.ClearToSend) { }

                    // Берём блок
                    var Block = PushingCanFrames.Take(BlockSize).ToList();

                    // Отправляем его либо сразу, либо с SeparationTime
                    if (SeparationTime == TimeSpan.Zero)
                        Socket.Send(Block);
                    else
                    {
                        foreach (var f in Block)
                        {
                            Socket.Send(f);
                            System.Threading.Thread.Sleep(SeparationTime);
                        }
                    }
                }
            }
            catch
            {
                this.Status = TpTransactionStatus.Error;
                throw;
            }
        }

        private void SendSingle()
        {
            try
            {
                var f = new SingleFrame(Buff);
                Socket.Send(f.GetCanFrame(TransmitDescriptor));
            }
            catch (Exception)
            {
                this.Status = TpTransactionStatus.Error;
                throw;
            }
        }

        private FlowControlFlag ProcessFlowControl(IEnumerable<CanFrame> FramesStream)
        {
            try
            {
                FlowControlFrame flowControl;
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                do
                {
                    stopWatch.Restart();
                    flowControl = FramesStream
                                .TakeWhile(f => stopWatch.Elapsed < Timeout)
                                .Where(f => f.GetIsoTpFrameType() == IsoTpFrameType.FlowControl)
                                .Select(f => (FlowControlFrame)f)
                                .FirstOrDefault();
                    if (flowControl == null) throw new IsoTpFlowControlTimeoutException();
                } while (flowControl.Flag == FlowControlFlag.Wait);

                // Считываем параметры отправки кадров
                this.BlockSize = flowControl.BlockSize != 0 ? flowControl.BlockSize : int.MaxValue;
                this.SeparationTime = flowControl.SeparationTime;

                // Выбрасываем ошибку, если принимающая сторона отказывается от транзакции
                if (flowControl.Flag == FlowControlFlag.Abort)
                    throw new IsoTpTransactionAbortedException("Принимающая сторона ответила флагом отмены транзакции");

                return flowControl.Flag;
            }
            catch (TimeoutException timeoutException) { throw new IsoTpFlowControlTimeoutException(timeoutException); }
        }

        int ConsecutiveFrameSent = 0;
        private IEnumerable<ConsecutiveFrame> GetConsFrames()
        {
            while (Pointer < Buff.Length)
            {
                var Data = new Byte[ConsecutiveFrame.DataCapacity];
                int DataLength = Math.Min(Data.Length, Buff.Length - Pointer);
                Buffer.BlockCopy(Buff, Pointer, Data, 0, DataLength);
                Pointer += Data.Length;
                yield return new ConsecutiveFrame(Data, ++ConsecutiveFrameSent & 0x0f);
            }
        }
        private FirstFrame GetFirstFrame()
        {
            var Data = new Byte[FirstFrame.DataCapacity];
            Buffer.BlockCopy(Buff, Pointer, Data, 0, Data.Length);
            Pointer += Data.Length;
            return new FirstFrame(Data, Buff.Length);
        }

    }
}
