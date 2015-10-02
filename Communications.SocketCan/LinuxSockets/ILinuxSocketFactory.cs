namespace Communications.SocketCan.LinuxSockets
{
    /// <summary>Фабрика по производству объектов типа <see cref="ILinuxSocket" />
    /// </summary>
    public interface ILinuxSocketFactory
    {
        /// <summary>Открывает Linux Socket</summary>
        /// <param name="InterfaceName">Имя интерфейса</param>
        ILinuxSocket OpenLinuxSocket(string InterfaceName);
    }
}
