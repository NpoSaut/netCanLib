using Communications.SocketCan.Lib;

namespace Communications.SocketCan.LinuxSockets
{
    /// <summary>Производит объекты типа <see cref="LinuxSocket" />, работающие через указанный интерфейс
    ///     <see cref="ISocketCanLibFacade" /> к библиотеке для работы с SocketCan</summary>
    public class LinuxSocketFactory : ILinuxSocketFactory
    {
        private readonly ISocketCanLibFacade _libFacade;

        public LinuxSocketFactory(ISocketCanLibFacade LibFacade, int RxBuffSize = 200, int TxBuffSize = 30)
        {
            _libFacade = LibFacade;
            this.RxBuffSize = RxBuffSize;
            this.TxBuffSize = TxBuffSize;
        }

        /// <summary>Размер буфера исходящих сообщений</summary>
        public int TxBuffSize { get; set; }

        /// <summary>Размер буфера входящих сообщений</summary>
        public int RxBuffSize { get; set; }

        /// <summary>Открывает Linux Socket</summary>
        /// <param name="InterfaceName">Имя интерфейса</param>
        public ILinuxSocket OpenLinuxSocket(string InterfaceName) { return new LinuxSocket(_libFacade, InterfaceName, RxBuffSize, TxBuffSize); }
    }
}
