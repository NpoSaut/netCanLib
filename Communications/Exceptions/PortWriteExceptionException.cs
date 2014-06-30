using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>Ошибка записи в порт</Summary>
    [Serializable]
    public class PortWriteExceptionException : PortException
    {
        public PortWriteExceptionException() : base("Ошибка записи в порт") { }
        public PortWriteExceptionException(Exception inner) : base("Ошибка записи в порт", inner) { }
        public PortWriteExceptionException(string message) : base(message) { }
        public PortWriteExceptionException(string message, Exception inner) : base(message, inner) { }

        protected PortWriteExceptionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
