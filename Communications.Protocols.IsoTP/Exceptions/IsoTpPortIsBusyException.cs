using System;
using System.Runtime.Serialization;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <Summary>В настоящий момент соединение используется</Summary>
    [Serializable]
    public class IsoTpPortIsBusyException : IsoTpProtocolException
    {
        public IsoTpPortIsBusyException() : base("В настоящий момент соединение используется") { }

        protected IsoTpPortIsBusyException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
