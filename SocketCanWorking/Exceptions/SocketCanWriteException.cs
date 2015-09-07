using System;
using System.Runtime.Serialization;

namespace SocketCanWorking.Exceptions
{
    /// <Summary>Ошибка при попытке записи в SocketCan.</Summary>
    [Serializable]
    public class SocketCanWriteException : SocketCanException
    {
        public SocketCanWriteException() : base("Ошибка при попытке записи в SocketCan") { }
        public SocketCanWriteException(int errorCode) : base(string.Format("Ошибка #{0} при попытке записи в SocketCan", errorCode)) { }
        public SocketCanWriteException(Exception inner) : base("Ошибка при попытке записи в SocketCan", inner) { }
        public SocketCanWriteException(string message) : base(message) { }
        public SocketCanWriteException(string message, Exception inner) : base(message, inner) { }

        protected SocketCanWriteException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
