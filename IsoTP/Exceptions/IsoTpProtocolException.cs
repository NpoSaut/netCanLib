using System;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <summary>
    /// Исключение, связанное с реализацией протокола ISO-TP
    /// </summary>
    [Serializable]
    public class IsoTpProtocolException : Exception
    {
        public IsoTpProtocolException() { }
        public IsoTpProtocolException(string message) : base(message) { }
        public IsoTpProtocolException(string message, Exception inner) : base(message, inner) { }
        protected IsoTpProtocolException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
