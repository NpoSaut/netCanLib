using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>
    /// Исключение при работе с сокетом
    /// </Summary>
    [Serializable]
    public class SocketException : System.ApplicationException
    {
        public SocketException() : base("Исключение при работе с сокетом") { }
        public SocketException(Exception inner) : base("Исключение при работе с сокетом", inner) { }
        public SocketException(string message) : base(message) { }
        public SocketException(string message, Exception inner) : base(message, inner) { }

        protected SocketException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}