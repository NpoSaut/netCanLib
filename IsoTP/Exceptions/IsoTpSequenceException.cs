using System;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <summary>
    /// Исключение, связанное с нарушением последовательности приёма кадров
    /// </summary>
    public class IsoTpSequenceException : IsoTpProtocolException
    {
        /// <summary>Ожидавшийся индекс</summary>
        public int ExpectedIndex { get; private set; }
        /// <summary>Полученый неожиданный индекс</summary>
        public int ReceivedIndex { get; private set; }
        /// <summary>Кадр с неожиданным индексом</summary>
        public IsoTpFrame Frame { get; private set; }

        public IsoTpSequenceException(int ExpectedIndex, int ReceivedIndex, IsoTpFrame Frame)
            : this(string.Format("Получено сообщение с индексом {0}, в то время как ожидалось сообщение с индексом {1} (Сообщение, содержащие неожиданный индекс: {2})",
                                 ReceivedIndex, ExpectedIndex, Frame),
                   ExpectedIndex, ReceivedIndex)
        { }

        public IsoTpSequenceException(int ExpectedIndex, int ReceivedIndex)
            : this(string.Format("Получено сообщение с индексом {0}, в то время как ожидалось сообщение с индексом {1}", ReceivedIndex, ExpectedIndex), ExpectedIndex, ReceivedIndex)
        { }
        public IsoTpSequenceException(String Message, int ExpectedIndex, int ReceivedIndex)
            : base(Message)
        {
            this.ExpectedIndex = ExpectedIndex;
            this.ReceivedIndex = ReceivedIndex;
        }
    }
}
