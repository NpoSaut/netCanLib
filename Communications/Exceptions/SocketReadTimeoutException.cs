using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>
    /// Превышено время ожидания при чтении из сокета
    /// </Summary>
    [Serializable]
    public class SocketReadTimeoutException : SocketTimeoutException
    { 
        public SocketReadTimeoutException() : base("Превышено время ожидания при чтении из сокета") { }
        public SocketReadTimeoutException(Exception inner) : base("Превышено время ожидания при чтении из сокета", inner) { }
        public SocketReadTimeoutException(string message) : base(message) { }
        public SocketReadTimeoutException(string message, Exception inner) : base(message, inner) { }

        protected SocketReadTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}