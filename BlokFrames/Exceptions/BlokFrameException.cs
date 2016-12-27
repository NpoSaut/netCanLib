using System;
using System.Runtime.Serialization;

namespace BlokFrames.Exceptions
{
    /// <Summary>Ошибка при работе с сообщениями системы БЛОК</Summary>
    [Serializable]
    public class BlokFrameException : ApplicationException
    {
        public BlokFrameException() : base("Ошибка при работе с сообщениями системы БЛОК") { }
        public BlokFrameException(Exception inner) : base("Ошибка при работе с сообщениями системы БЛОК", inner) { }
        public BlokFrameException(string message) : base(message) { }
        public BlokFrameException(string message, Exception inner) : base(message, inner) { }

        protected BlokFrameException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
