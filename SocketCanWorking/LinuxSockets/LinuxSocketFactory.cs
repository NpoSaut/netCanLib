using System;
using SocketCanWorking.Lib;

namespace SocketCanWorking.LinuxSockets
{
    /// <summary>
    ///     Производит объекты типа <see cref="LinuxSocket" />, работающие через указанный интерфейс
    ///     <see cref="ISocketCanLibFacade" /> к библиотеке для работы с SocketCan
    /// </summary>
    public class LinuxSocketFactory : ILinuxSocketFactory
    {
        private readonly ISocketCanLibFacade _libFacade;
        public LinuxSocketFactory(ISocketCanLibFacade LibFacade) { _libFacade = LibFacade; }

        public ILinuxSocket OpenLinuxSocket(String InterfaceName) { return new LinuxSocket(InterfaceName, _libFacade); }
    }
}
