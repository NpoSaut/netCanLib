using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>Попытка повторно открыть сокет в порту, поддерживающем работу только с одним сокетом</Summary>
    [Serializable]
    public class SingleSocketReopenException : PortException
    {
        public SingleSocketReopenException(string PortName)
            : base("Попытка повторно открыть сокет в порту, поддерживающем работу только с одним сокетом")
        {
            this.PortName = PortName;
        }

        public SingleSocketReopenException(string PortName, Exception inner)
            : base("Попытка повторно открыть сокет в порту, поддерживающем работу только с одним сокетом", inner)
        {
            this.PortName = PortName;
        }

        public SingleSocketReopenException(string PortName, string message) : base(message)
        {
            this.PortName = PortName;
        }

        public SingleSocketReopenException(string PortName, string message, Exception inner) : base(message, inner)
        {
            this.PortName = PortName;
        }

        protected SingleSocketReopenException(
            string PortName,
            SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.PortName = PortName;
        }

        /// <summary>Имя порта</summary>
        public String PortName { get; private set; }
    }
}
