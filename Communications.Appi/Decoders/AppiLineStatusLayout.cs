namespace Communications.Appi.Decoders
{
    /// <summary>Расположение сведений о Can-линии в буфере ответа АППИ</summary>
    public class AppiLineStatusLayout
    {
        /// <summary>Инициализирует новый экземпляр класса <see cref="T:AppiLineStatusLayout" />.</summary>
        /// <param name="FramesBodyOffset">Положение тела сообщений</param>
        /// <param name="FramesCountOffset">Положение количества сообщений</param>
        /// <param name="BaudRateOffset">Положение данных о выбранной скорости линии</param>
        /// <param name="SendQueueOffset">Положение размера очереди на отправку по этой линии</param>
        /// <param name="SendErrorIndex">Положение индекса ошибок отправки</param>
        /// <param name="ReceiveErrorIndex">Положение индекса ошибок приёма</param>
        public AppiLineStatusLayout(int FramesBodyOffset, int FramesCountOffset, int BaudRateOffset, int SendQueueOffset, int SendErrorIndex,
                                    int ReceiveErrorIndex)
        {
            this.FramesBodyOffset = FramesBodyOffset;
            this.FramesCountOffset = FramesCountOffset;
            this.BaudRateOffset = BaudRateOffset;
            this.SendQueueOffset = SendQueueOffset;
            this.SendErrorIndex = SendErrorIndex;
            this.ReceiveErrorIndex = ReceiveErrorIndex;
        }

        /// <summary>Положение тела сообщений</summary>
        public int FramesBodyOffset { get; private set; }

        /// <summary>Положение количества сообщений</summary>
        public int FramesCountOffset { get; private set; }

        /// <summary>Положение данных о выбранной скорости линии</summary>
        public int BaudRateOffset { get; private set; }

        /// <summary>Положение размера очереди на отправку по этой линии</summary>
        public int SendQueueOffset { get; private set; }

        /// <summary>Положение индекса ошибок отправки</summary>
        public int SendErrorIndex { get; private set; }

        /// <summary>Положение индекса ошибок приёма</summary>
        public int ReceiveErrorIndex { get; private set; }
    }
}
