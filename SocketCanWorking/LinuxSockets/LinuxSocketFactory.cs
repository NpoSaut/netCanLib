using SocketCanWorking.Lib;

namespace SocketCanWorking.LinuxSockets
{
    /// <summary>Производит объекты типа <see cref="LinuxSocket" />, работающие через указанный интерфейс
    ///     <see cref="ISocketCanLibFacade" /> к библиотеке для работы с SocketCan</summary>
    public class LinuxSocketFactory : ILinuxSocketFactory
    {
        private readonly ISocketCanLibFacade _libFacade;
        public LinuxSocketFactory(ISocketCanLibFacade LibFacade) { _libFacade = LibFacade; }

        /// <summary>Открывает Linux Socket</summary>
        /// <param name="InterfaceName">Имя интерфейса</param>
        /// <param name="TxBuffSize">Размер буфера исходящих сообщений</param>
        /// <param name="RxBuffSize">Размер буфера входящих сообщений</param>
        public ILinuxSocket OpenLinuxSocket(string InterfaceName, int RxBuffSize, int TxBuffSize)
        {
            return new LinuxSocket(_libFacade, InterfaceName, RxBuffSize, TxBuffSize);
        }
    }
}
