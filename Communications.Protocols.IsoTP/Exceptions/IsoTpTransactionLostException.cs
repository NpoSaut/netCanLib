using System;
using System.Runtime.Serialization;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <Summary>Транзакция потерена (во время выполнения текущей транзакции была инициирована  новая транзакция)</Summary>
    [Serializable]
    public class IsoTpTransactionLostException : IsoTpProtocolException
    {
        public IsoTpTransactionLostException() : base("Транзакция потерена (во время выполнения текущей транзакции была инициирована  новая транзакция)") { }

        protected IsoTpTransactionLostException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
