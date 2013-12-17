using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>
    /// Превышено время ожидания при отправке в сокет
    /// </Summary>
    [Serializable]
    public class SocketSendTimeoutException : SocketTimeoutException
    { 
        public SocketSendTimeoutException() : base("Превышено время ожидания при отправке в сокет") { }
        public SocketSendTimeoutException(Exception inner) : base("Превышено время ожидания при отправке в сокет", inner) { }
        public SocketSendTimeoutException(string message) : base(message) { }
        public SocketSendTimeoutException(string message, Exception inner) : base(message, inner) { }

        protected SocketSendTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}