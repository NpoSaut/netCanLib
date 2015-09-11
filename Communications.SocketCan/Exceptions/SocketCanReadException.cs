using System;
using System.Runtime.Serialization;

namespace Communications.SocketCan.Exceptions
{
    /// <Summary>Ошибка при чтении из SocketCan.</Summary>
    [Serializable]
    public class SocketCanReadException : SocketCanException
    {
        public SocketCanReadException() : base("Ошибка при чтении из SocketCan") { }
        public SocketCanReadException(int errorCode) : base(string.Format("Ошибка #{0} при чтении из SocketCan", errorCode)) { }
        public SocketCanReadException(Exception inner) : base("Ошибка при чтении из SocketCan", inner) { }
        public SocketCanReadException(string message) : base(message) { }
        public SocketCanReadException(string message, Exception inner) : base(message, inner) { }

        protected SocketCanReadException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
