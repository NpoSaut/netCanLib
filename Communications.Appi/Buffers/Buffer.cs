using System.Collections.Generic;

namespace Communications.Appi.Buffers
{
    /// <summary>Буфер АППИ</summary>
    public abstract class Buffer
    {
        protected Buffer(int SequentialNumber) { this.SequentialNumber = SequentialNumber; }

        /// <summary>Сквозной номер буфера</summary>
        public int SequentialNumber { get; private set; }
    }

    /// <summary>Буфер АППИ со списком принятых сообщений</summary>
    /// <typeparam name="TLineKey">Идентификатор линии</typeparam>
    public abstract class MessagesBuffer<TLineKey> : Buffer
    {
        protected MessagesBuffer(int SequentialNumber, IDictionary<TLineKey, AppiLineStatus> LineStatuses) : base(SequentialNumber)
        {
            this.LineStatuses = LineStatuses;
        }

        /// <summary>Состояния линий</summary>
        public IDictionary<TLineKey, AppiLineStatus> LineStatuses { get; private set; }
    }
}
