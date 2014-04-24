using System;
using System.Runtime.Serialization;

namespace WinusbSocket.Exceptions
{
    /// <Summary>Ошибка при попытке подключения к USB-устройству</Summary>
    [Serializable]
    public class WinusbCantOpenDeviceException : WinusbPortException
    {
        public WinusbCantOpenDeviceException() : base("Ошибка при попытке подключения к USB-устройству") { }

        public WinusbCantOpenDeviceException(Exception inner)
            : base("Ошибка при попытке подключения к USB-устройству", inner) { }

        public WinusbCantOpenDeviceException(string message) : base(message) { }
        public WinusbCantOpenDeviceException(string message, Exception inner) : base(message, inner) { }

        protected WinusbCantOpenDeviceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}
