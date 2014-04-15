using System;
using System.Runtime.Serialization;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <Summary>Превышено время ожидания начала транзакции</Summary>
    [Serializable]
    public class IsoTpReceiveTimeoutException : IsoTpTimeoutException
    {
        public IsoTpReceiveTimeoutException()
            : base("Превышено время ожидания начала транзакции") { }

        public IsoTpReceiveTimeoutException(Exception inner)
            : base("Превышено время ожидания начала транзакции", inner) { }

        public IsoTpReceiveTimeoutException(string message) : base(message) { }

        public IsoTpReceiveTimeoutException(string message, Exception inner) : base(message, inner) { }

        protected IsoTpReceiveTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
