using System;
using System.Runtime.Serialization;

namespace SocketCanWorking.Exceptions
{
    /// <Summary>
    /// Ошибка при попытке открыть сокет
    /// </Summary>
    [Serializable]
    public class SocketCanOpenException : SocketCanException
    {
        public SocketCanOpenException() : base("Ошибка при попытке открыть сокет") { }
        public SocketCanOpenException(int errorCode) : base(string.Format("Ошибка #{0} при попытке открыть сокет", errorCode)) { }
        public SocketCanOpenException(Exception inner) : base("Ошибка при попытке открыть сокет", inner) { }
        public SocketCanOpenException(string message) : base(message) { }
        public SocketCanOpenException(string message, Exception inner) : base(message, inner) { }

        protected SocketCanOpenException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}