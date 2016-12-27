using System;
using System.Runtime.Serialization;

namespace BlokFrames.Exceptions
{
    /// <Summary>Ошибка при кодировании/декодировании сообщений протокола системы БЛОК</Summary>
    [Serializable]
    public class BlokFrameProtocolException : BlokFrameException
    {
        public BlokFrameProtocolException() : base("Ошибка при кодировании/декодировании сообщений протокола системы БЛОК") { }
        public BlokFrameProtocolException(Exception inner) : base("Ошибка при кодировании/декодировании сообщений протокола системы БЛОК", inner) { }
        public BlokFrameProtocolException(string message) : base(message) { }
        public BlokFrameProtocolException(string message, Exception inner) : base(message, inner) { }

        protected BlokFrameProtocolException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
