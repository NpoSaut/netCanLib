using System;
using System.IO;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;

namespace Communications.Protocols.IsoTP
{
    public interface ITransactionContext
    {
        /// <summary>Размер одного блока</summary>
        int BlockSize { get; }
        /// <summary>Поток данных (входящих или исходящих)</summary>
        MemoryStream DataStream { get; }

        void OnTransactionReady();
        void SetNextState(IsoTpState NextState);
        void SendControlFrame();
        void CreateBuffer(int PacketSize);
        void WriteToBuffer(byte[] Bytes);
        bool IsBufferFull();
    }

    class MemoryTransactionContext : ITransactionContext
    {
        public TimeSpan SeparationTime { get; private set; }
        public int BlockSize { get; private set; }
        public MemoryStream DataStream { get; private set; }
        public ICanFlow CanFlow { get; private set; }
        public int SenderDescriptor { get; private set; }
        public int ReceiverDescriptor { get; private set; }

        public MemoryTransactionContext(TimeSpan SeparationTime, int BlockSize, ICanFlow CanFlow,
                                  int SenderDescriptor, int ReceiverDescriptor)
        {
            this.ReceiverDescriptor = ReceiverDescriptor;
            this.SenderDescriptor = SenderDescriptor;
            this.CanFlow = CanFlow;
            this.BlockSize = BlockSize;
            this.SeparationTime = SeparationTime;
        }


        public void SendControlFrame()
        {
            var flowControlFrame = new FlowControlFrame(FlowControlFlag.ClearToSend,
                                                        (byte)BlockSize,
                                                        SeparationTime);
            CanFlow.Send(flowControlFrame.GetCanFrame(SenderDescriptor));
        }


        public void OnTransactionReady() { throw new NotImplementedException(); }
        public void SetNextState(IsoTpState NextState) { throw new NotImplementedException(); }

        public void CreateBuffer(int PacketSize)
        {
            DataStream = new MemoryStream();
            DataStream.SetLength(PacketSize);
        }
        public void WriteToBuffer(byte[] Bytes) { DataStream.Write(Bytes, 0, Bytes.Length); }
        public bool IsBufferFull() { return DataStream.Position == DataStream.Length - 1; }
    }
}