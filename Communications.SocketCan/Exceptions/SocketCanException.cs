using System;
using System.Runtime.Serialization;

namespace Communications.SocketCan.Exceptions
{
    /// <Summary>Исключение при работе с SocketCan.</Summary>
    [Serializable]
    public class SocketCanException : ApplicationException
    {
        public SocketCanException() : base("Исключение при работе с SocketCan") { }
        public SocketCanException(Exception inner) : base("Исключение при работе с SocketCan", inner) { }
        public SocketCanException(string message) : base(message) { }
        public SocketCanException(string message, Exception inner) : base(message, inner) { }

        protected SocketCanException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
