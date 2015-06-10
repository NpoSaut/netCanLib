using System;
using System.Runtime.Serialization;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <Summary>Было получено несоответствующее сообщение</Summary>
    [Serializable]
    public class IsoTpWrongFrameException : IsoTpProtocolException
    {
        public IsoTpWrongFrameException() : base("Было получено несоответствующее сообщение") { }

        public IsoTpWrongFrameException(IsoTpFrame ReceivedFrame, Type ExpectedFrameType)
            : base(string.Format("Было получено ISO-TP сообщение типа {0} ({2}), в то время, как ожидалось сообщение типа {1}",
                              ReceivedFrame.GetType().Name, ExpectedFrameType, ReceivedFrame)) { }

        public IsoTpWrongFrameException(Exception inner) : base("Было получено несоответствующее сообщение", inner) { }
        public IsoTpWrongFrameException(string message) : base(message) { }
        public IsoTpWrongFrameException(string message, Exception inner) : base(message, inner) { }

        protected IsoTpWrongFrameException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
