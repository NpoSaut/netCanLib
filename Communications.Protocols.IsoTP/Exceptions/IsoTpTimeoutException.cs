using System;
using System.Runtime.Serialization;
using Communications.Protocols.IsoTP.StateManagers;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <Summary>Превышено время ожидания чтения ISO-TP фрейма</Summary>
    [Serializable]
    public class IsoTpTimeoutException : IsoTpProtocolException
    {
        private readonly TimeoutReason _reason;
        public IsoTpTimeoutException(TimeoutReason Reason) : base(string.Format("Превышено время ожидания чтения ISO-TP фрейма ({0})", Reason)) { _reason = Reason; }
        public IsoTpTimeoutException(Exception inner) : base("Превышено время ожидания чтения ISO-TP фрейма", inner) { }
        public IsoTpTimeoutException(string message) : base(message) { }
        public IsoTpTimeoutException(string message, Exception inner) : base(message, inner) { }

        protected IsoTpTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
