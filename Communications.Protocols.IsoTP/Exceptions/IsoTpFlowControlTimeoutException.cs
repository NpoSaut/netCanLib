using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Communications.Protocols.IsoTP.Exceptions
{
    /// <Summary>
    /// Превышено время ожидания FlowControl сообщения
    /// </Summary>
    [Serializable]
    public class IsoTpFlowControlTimeoutException : IsoTpProtocolException
    {
        public IsoTpFlowControlTimeoutException() : base("Превышено время ожидания FlowControl сообщения")
        {
        }

        public IsoTpFlowControlTimeoutException(Exception inner) : base("Превышено время ожидания FlowControl сообщения", inner)
        {
        }

        public IsoTpFlowControlTimeoutException(string message) : base(message)
        {
        }

        public IsoTpFlowControlTimeoutException(string message, Exception inner) : base(message, inner)
        {
        }

        protected IsoTpFlowControlTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
