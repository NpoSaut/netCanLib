using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>
    /// Попытка доступа к закрытому сокету
    /// </Summary>
    [Serializable]
    public class SocketClosedException : SocketException
    { 
        public SocketClosedException() : base("Попытка доступа к закрытому сокету") { }
        public SocketClosedException(Exception inner) : base("Попытка доступа к закрытому сокету", inner) { }
        public SocketClosedException(string message) : base(message) { }
        public SocketClosedException(string message, Exception inner) : base(message, inner) { }

        protected SocketClosedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}