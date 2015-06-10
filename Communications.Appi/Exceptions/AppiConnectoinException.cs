using System;

namespace Communications.Appi.Exceptions
{
    [Serializable]
    public class AppiConnectoinException : AppiException
    {
        public AppiConnectoinException() : base("Ошибка связи с АППИ") { }
        public AppiConnectoinException(Exception inner) : base("Ошибка связи с АППИ", inner) { }
        public AppiConnectoinException(string message) : base(message) { }
        public AppiConnectoinException(string message, Exception inner) : base(message, inner) { }
        protected AppiConnectoinException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}