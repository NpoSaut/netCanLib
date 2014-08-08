namespace SocketCanWorking.LinuxSockets
{
    /// <summary>Фабрика по производству объектов типа <see cref="ILinuxSocket" />
    /// </summary>
    public interface ILinuxSocketFactory
    {
        /// <summary>Открывает Linux Socket</summary>
        /// <param name="InterfaceName">Имя интерфейса</param>
        /// <param name="TxBuffSize">Размер буфера исходящих сообщений</param>
        /// <param name="RxBuffSize">Размер буфера входящих сообщений</param>
        ILinuxSocket OpenLinuxSocket(string InterfaceName, int RxBuffSize, int TxBuffSize);
    }
}
