using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>Ошибка при работе с портом</Summary>
    [Serializable]
    public abstract class PortException : CommunicationException
    {
        public PortException() : base("Ошибка при работе с портом") { }
        public PortException(Exception inner) : base("Ошибка при работе с портом", inner) { }
        public PortException(string message) : base(message) { }
        public PortException(string message, Exception inner) : base(message, inner) { }

        protected PortException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
