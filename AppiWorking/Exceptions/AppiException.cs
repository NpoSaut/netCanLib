using System;

namespace Communications.Appi.Exceptions
{
    [Serializable]
    public class AppiException : ApplicationException
    {
        public AppiException() { }
        public AppiException(string message) : base(message) { }
        public AppiException(string MessageFormat, params object[] MessageArguments) : base(string.Format(MessageFormat, MessageArguments)) { }
        public AppiException(string message, Exception inner) : base(message, inner) { }
        protected AppiException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
