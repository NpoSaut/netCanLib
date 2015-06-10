using System;
using System.Runtime.Serialization;

namespace Communications.Appi.Exceptions
{
    /// <Summary>Устройство уже используется</Summary>
    [Serializable]
    public class AppiDeviceOccupiedException : AppiException
    {
        public AppiDeviceOccupiedException() : base("Устройство уже используется") { }
        public AppiDeviceOccupiedException(Exception inner) : base("Устройство уже используется", inner) { }
        public AppiDeviceOccupiedException(string message) : base(message) { }
        public AppiDeviceOccupiedException(string message, Exception inner) : base(message, inner) { }

        protected AppiDeviceOccupiedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
