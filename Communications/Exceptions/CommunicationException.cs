using System;
using System.Runtime.Serialization;

namespace Communications.Exceptions
{
    /// <Summary>Ошибка при работе с устройством коммуникации</Summary>
    [Serializable]
    public class CommunicationException : ApplicationException
    {
        public CommunicationException() : base("Ошибка при работе с устройством коммуникации") { }
        public CommunicationException(Exception inner) : base("Ошибка при работе с устройством коммуникации", inner) { }
        public CommunicationException(string message) : base(message) { }
        public CommunicationException(string message, Exception inner) : base(message, inner) { }

        protected CommunicationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
