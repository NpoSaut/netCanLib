using System;
using System.Collections.Generic;
using Communications.Can;

namespace SocketCanWorking
{
    /// <summary>Содержит методы для работы с Linux-овым SocketCan.</summary>
    public static class SocketCan
    {
        private static readonly Dictionary<String, CanPort> OpenedPorts = new Dictionary<string, CanPort>();

        /// <summary>Открывает SocketCan-порт.</summary>
        /// <param name="InterfaceName">Системное имя порта.</param>
        public static CanPort OpenPort(String InterfaceName)
        {
            lock (OpenedPorts)
            {
                if (OpenedPorts.ContainsKey(InterfaceName)) return OpenedPorts[InterfaceName];
                CanPort res = OpenNewPort(InterfaceName);
                OpenedPorts.Add(InterfaceName, res);
                return res;
            }
        }

        private static CanPort OpenNewPort(string InterfaceName)
        {
            var facade = new SocketCanFacade(InterfaceName);
            var res = new CanPort(InterfaceName, facade, facade);
            return res;
        }
    }
}
