using System;
using System.Runtime.Serialization;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <Summary>ISO-TP порт был закрыт</Summary>
    [Serializable]
    public class IsoTpPortClosedException : IsoTpProtocolException
    {
        public IsoTpPortClosedException() : base("ISO-TP порт был закрыт") { }

        protected IsoTpPortClosedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
