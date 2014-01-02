using System;
using System.IO;
using Communications.Can;

namespace Communications.Protocols.IsoTP
{
    internal interface ITransactionContext
    {
        /// <summary>Задержка перед отправкой очередного фрейма</summary>
        TimeSpan SeparationTime { get; }
        /// <summary>Размер одного блока</summary>
        int BlockSize { get; }
        /// <summary>Поток данных (входящих или исходящих)</summary>
        MemoryStream DataStream { get; }
        /// <summary>Поток Can-сообщений</summary>
        ICanFlow CanFlow { get; }
        /// <summary>Дескриптор отправляющей стороны</summary>
        int SenderDescriptor { get; }
        /// <summary>Дескриптор принимающей стороны</summary>
        int ReceiverDescriptor { get; }
    }

    class MemoryTransactionContext : ITransactionContext
    {
        public TimeSpan SeparationTime { get; private set; }
        public int BlockSize { get; private set; }
        public MemoryStream DataStream { get; private set; }
        public ICanFlow CanFlow { get; private set; }
        public int SenderDescriptor { get; private set; }
        public int ReceiverDescriptor { get; private set; }

        public MemoryTransactionContext(TimeSpan SeparationTime, int BlockSize, int DataLength, ICanFlow CanFlow,
                                  int SenderDescriptor, int ReceiverDescriptor)
        {
            this.ReceiverDescriptor = ReceiverDescriptor;
            this.SenderDescriptor = SenderDescriptor;
            this.CanFlow = CanFlow;
            this.BlockSize = BlockSize;
            this.SeparationTime = SeparationTime;
            DataStream = new MemoryStream(DataLength);
        }
    }
}