using Communications.Can;
using Communications.SocketCan.LinuxSockets;

namespace Communications.SocketCan
{
    public class LinuxSocketCanPortProvider : ICanPortProvider
    {
        private readonly string _interfaceName;
        private readonly ILinuxSocketFactory _linuxSocketFactory;

        public LinuxSocketCanPortProvider(string InterfaceName, ILinuxSocketFactory LinuxSocketFactory)
        {
            _interfaceName = InterfaceName;
            _linuxSocketFactory = LinuxSocketFactory;
        }

        public ICanPort OpenPort()
        {
            return new SocketCanPort(_interfaceName,
                                     _linuxSocketFactory.OpenLinuxSocket(_interfaceName),
                                     _linuxSocketFactory.OpenLinuxSocket(_interfaceName));
        }
    }
}
