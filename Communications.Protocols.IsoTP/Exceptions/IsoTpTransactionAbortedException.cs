using System;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <summary>
    /// Исключение, возникающее, если транзакция была отменена принимающей стороной
    /// </summary>
    public class IsoTpTransactionAbortedException : IsoTpProtocolException
    {
        public IsoTpTransactionAbortedException()
            : base()
        { }
        public IsoTpTransactionAbortedException(String Message)
            : base(Message)
        { }
        public IsoTpTransactionAbortedException(String Message, Exception InternalException)
            : base(Message, InternalException)
        { }
    }
}
