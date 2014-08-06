using System;
using Communications;
using Communications.Can;
using SocketCanWorking.Lib;
using SocketCanWorking.LinuxSockets;

namespace SocketCanWorking
{
    /// <summary>Содержит методы для работы с Linux-овым SocketCan.</summary>
    public static class SocketCan
    {
        private static readonly ILinuxSocketFactory linuxSocketFactory;

        static SocketCan() { linuxSocketFactory = new LinuxSocketFactory(new SocketCanLibFacade()); }

        /// <summary>Открывает SocketCan-порт.</summary>
        /// <param name="InterfaceName">Системное имя интерфейса.</param>
        public static IPort<ICanSocket> OpenDirectPort(String InterfaceName) { return new DirectSocketCanPort(InterfaceName, linuxSocketFactory); }
    }
}
