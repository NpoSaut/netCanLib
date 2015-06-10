using System;
using System.Runtime.Serialization;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <Summary>Превышено время ожидания чтения ISO-TP фрейма</Summary>
    [Serializable]
    public class IsoTpTimeoutException : IsoTpProtocolException
    {
        public IsoTpTimeoutException() : base("Превышено время ожидания чтения ISO-TP фрейма") { }
        public IsoTpTimeoutException(Exception inner) : base("Превышено время ожидания чтения ISO-TP фрейма", inner) { }
        public IsoTpTimeoutException(string message) : base(message) { }
        public IsoTpTimeoutException(string message, Exception inner) : base(message, inner) { }

        protected IsoTpTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
