using Communications.Can;
using Communications.SocketCan.Lib;
using Communications.SocketCan.LinuxSockets;

namespace Communications.SocketCan
{
    public class LinuxSocketCanPortProvider : ICanPortProvider
    {
        private readonly string _interfaceName;
        private readonly int _rxBuffSize;
        private readonly ISocketCanLibFacade _socketCanLibFacade;
        private readonly int _txBuffSize;

        public LinuxSocketCanPortProvider(string InterfaceName, ISocketCanLibFacade SocketCanLibFacade, int RxBuffSize = 200, int TxBuffSize = 30)
        {
            _interfaceName = InterfaceName;
            _socketCanLibFacade = SocketCanLibFacade;
            _rxBuffSize = RxBuffSize;
            _txBuffSize = TxBuffSize;
        }

        public ICanPort OpenPort()
        {
            return new SocketCanPort(_interfaceName,
                                     new LinuxSocket(_socketCanLibFacade, _interfaceName, _rxBuffSize, _txBuffSize),
                                     new LinuxSocket(_socketCanLibFacade, _interfaceName, _rxBuffSize, _txBuffSize));
        }
    }
}
