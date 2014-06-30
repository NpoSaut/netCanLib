using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>Запись в порт была прервана</Summary>
    [Serializable]
    public class PortWriteAbortedException : PortWriteExceptionException
    {
        public PortWriteAbortedException() : base("Запись в порт была прервана") { }
        public PortWriteAbortedException(Exception inner) : base("Запись в порт была прервана", inner) { }
        public PortWriteAbortedException(string message) : base(message) { }
        public PortWriteAbortedException(string message, Exception inner) : base(message, inner) { }

        protected PortWriteAbortedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
