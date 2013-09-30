using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Protocols.IsoTP
{
    [Serializable]
    public class DescriptorNotInFlowException : IsoTpProtocolException
    {
        public int? Descriptor { get; private set; }

        public DescriptorNotInFlowException() { }
        public DescriptorNotInFlowException(string message) : base(message) { }
        public DescriptorNotInFlowException(string message, Exception inner) : base(message, inner) { }
        protected DescriptorNotInFlowException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public DescriptorNotInFlowException(int Descriptor)
            : this(string.Format("Дескриптор {0:X4} не входит в список отслеживаемых данным потоком дескрипторов", Descriptor))
        {
            this.Descriptor = Descriptor;
        }
    }
}
