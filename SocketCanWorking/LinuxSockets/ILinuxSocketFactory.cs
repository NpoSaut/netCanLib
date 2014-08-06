using System;

namespace SocketCanWorking.LinuxSockets
{
    /// <summary>Фабрика по производству объектов типа <see cref="ILinuxSocket" />
    /// </summary>
    public interface ILinuxSocketFactory
    {
        ILinuxSocket OpenLinuxSocket(String InterfaceName);
    }
}