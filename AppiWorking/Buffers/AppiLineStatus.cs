using System.Collections.Generic;
using Communications.Can;

namespace Communications.Appi.Buffers
{
    /// <summary>Информация о статусе линии АППИ CAN</summary>
    public class AppiLineStatus
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.Object"/>.
        /// </summary>
        public AppiLineStatus(int BaudRate, IEnumerable<CanFrame> Frames, int SendQueueSize, int SendErrorsIndex, int ReceiveErrorsIndex)
        {
            this.BaudRate = BaudRate;
            this.Frames = Frames;
            this.SendQueueSize = SendQueueSize;
            this.SendErrorsIndex = SendErrorsIndex;
            this.ReceiveErrorsIndex = ReceiveErrorsIndex;
        }

        /// <summary>Скорость обмена по линии</summary>
        public int BaudRate { get; private set; }

        /// <summary>Входящие сообщения по линии</summary>
        public IEnumerable<CanFrame> Frames { get; private set; }

        /// <summary>Размер очереди исходящих сообщений</summary>
        public int SendQueueSize { get; private set; }

        /// <summary>Индекс ошибок отправки</summary>
        public int SendErrorsIndex { get; private set; }

        /// <summary>Индекс ошибок приёма</summary>
        public int ReceiveErrorsIndex { get; private set; }
    }
}
