using Communications;
using Communications.Can;
using Communications.Ports;
using SocketCanWorking.LinuxSockets;
using SocketCanWorking.ReadersWriters;

namespace SocketCanWorking
{
    /// <summary>Порт, открывающий по одному Linux-сокету на каждый <see cref="ISocket{TDatagram}"/>, напрямую реализующий работу с Linux SocketCan</summary>
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
            ILinuxSocket linuxSocket = _linuxSocketFactory.OpenLinuxSocket(_interfaceName);
            return
                new SocketCanSocket(
                    string.Format("Linux сокет на {0}", _interfaceName),
                    new DirectSocketCanReader(linuxSocket),
                    new DirectSocketCanWriter(linuxSocket));
        }
    }
}
