using System;
using System.Runtime.Serialization;
using Communications.Exceptions;

namespace WinusbSocket.Exceptions
{
    /// <Summary>Ошибка при работе с портом WinUSB</Summary>
    [Serializable]
    public class WinusbPortException : PortException
    {
        public WinusbPortException() : base("Ошибка при работе с портом WinUSB") { }
        public WinusbPortException(Exception inner) : base("Ошибка при работе с портом WinUSB", inner) { }
        public WinusbPortException(string message) : base(message) { }
        public WinusbPortException(string message, Exception inner) : base(message, inner) { }

        protected WinusbPortException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
