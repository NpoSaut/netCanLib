using System;
using System.Runtime.Serialization;

namespace Communications.Appi.Exceptions
{
    /// <Summary>
    /// Передача была отменена
    /// </Summary>
    [Serializable]
    public class TransferAbortedException : AppiException
    {
        public TransferAbortedException() : base("Передача была отменена") { }
        public TransferAbortedException(Exception inner) : base("Передача была отменена", inner) { }
        public TransferAbortedException(string message) : base(message) { }
        public TransferAbortedException(string message, Exception inner) : base(message, inner) { }

        protected TransferAbortedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}