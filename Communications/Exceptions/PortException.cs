using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>Исключение, связанное с работой с портом</Summary>
    [Serializable]
    public abstract class PortException : ApplicationException
    {
        public PortException() : base("Исключение, связанное с работой с портом") { }
        public PortException(Exception inner) : base("Исключение, связанное с работой с портом", inner) { }
        public PortException(string message) : base(message) { }
        public PortException(string message, Exception inner) : base(message, inner) { }

        protected PortException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
