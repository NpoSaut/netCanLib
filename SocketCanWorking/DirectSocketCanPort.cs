using Communications.Can;
using Communications.Ports;

namespace SocketCanWorking
{
    /// <summary>Порт, работающий с <see cref="DirectSocketCanSocket" />, напрямую реализующими работу с Linux SocketCan</summary>
    public class DirectSocketCanPort : PortBase<ICanSocket, CanFrame>
    {
        private readonly string _interfaceName;
        private readonly ILinuxSocketFactory _linuxSocketFactory;

        public DirectSocketCanPort(string InterfaceName, ILinuxSocketFactory LinuxSocketFactory) : base(string.Format("SocketCan на {0}", InterfaceName))
        {
            _interfaceName = InterfaceName;
            _linuxSocketFactory = LinuxSocketFactory;
        }

        /// <summary>Создаёт новый сокет, который будет зарегистрирован в порту и возвращён функцией OpenSocket</summary>
        protected override ICanSocket ImplementOpenSocket()
        {
            return new DirectSocketCanSocket(string.Format("Linux сокет на {0}", _interfaceName), _linuxSocketFactory.OpenLinuxSocket(_interfaceName));
        }
    }
}
