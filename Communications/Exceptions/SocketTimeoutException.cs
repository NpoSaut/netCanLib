using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>
    /// Превышено время ожидания при работе с сокетом
    /// </Summary>
    [Serializable]
    public class SocketTimeoutException : SocketException
    {
        public SocketTimeoutException() : base("Превышено время ожидания при работе с сокетом") { }
        public SocketTimeoutException(Exception inner) : base("Превышено время ожидания при работе с сокетом", inner) { }
        public SocketTimeoutException(string message) : base(message) { }
        public SocketTimeoutException(string message, Exception inner) : base(message, inner) { }

        protected SocketTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}