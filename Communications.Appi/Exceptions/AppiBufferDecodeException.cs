using System;

namespace Communications.Appi.Exceptions
{
    [Serializable]
    public class AppiBufferDecodeException : AppiException
    {
        public AppiBufferDecodeException() : base("Ошибка декодирования буфера") { }
        public AppiBufferDecodeException(string message) : base("Ошибка декодирования буфера: " + message) { }
        public AppiBufferDecodeException(string MessageFormat, params object[] MessageArguments) : base("Ошибка декодирования буфера: " + MessageFormat, MessageArguments) { }
        public AppiBufferDecodeException(string message, Exception inner) : base("Ошибка декодирования буфера: " + message, inner) { }
        protected AppiBufferDecodeException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}