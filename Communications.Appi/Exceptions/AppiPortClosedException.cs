using System;
using System.Runtime.Serialization;

namespace Communications.Appi.Exceptions
{
    /// <Summary>Порт уже был закрыт</Summary>
    [Serializable]
    public class AppiPortClosedException : AppiException
    {
        public AppiPortClosedException() : base("Порт уже был закрыт") { }

        protected AppiPortClosedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
