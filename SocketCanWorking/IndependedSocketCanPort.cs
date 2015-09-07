using Communications.Can;
using Communications.Ports;
using SocketCanWorking.LinuxSockets;
using SocketCanWorking.ReadersWriters;

namespace SocketCanWorking
{
    /// <summary>
    ///     Can-порт, работающий через Linux SocketCan с реализацией блокирующей отправки посредством мониторинга входящих
    ///     сообщений с сокета
    /// </summary>
    /// <remarks>
    ///     Этот порт открывает по два Linux-сокета на каждый открытый свой сокет. Один из сокетов служит для чтения
    ///     сообщений, другой - для отправки и контроля отправки сообщений: функция отправки сообщений блокируется и производит
    ///     чтение из своего сокета в ожидании Loopback-фреймов. Разблокируется (или отправляет в буфер следующий кусок
    ///     сообщений) только после того, как все сообщения из текущего куска были доставлены.
    /// </remarks>
    public class IndependedSocketCanPort : PortBase<ICanSocket, CanFrame>
    {
        private readonly string _interfaceName;
        private readonly ILinuxSocketFactory _linuxSocketFactory;

        public IndependedSocketCanPort(string InterfaceName, ILinuxSocketFactory LinuxSocketFactory)
            : base(string.Format("IdpSocketCan on {0}", InterfaceName))
        {
            _interfaceName = InterfaceName;
            _linuxSocketFactory = LinuxSocketFactory;

            RxBuffSize = 200;
            TxBuffSize = 50;
        }

        /// <summary>Размер буфера исходящих сообщений</summary>
        public int TxBuffSize { get; set; }

        /// <summary>Размер буфера входящих сообщений</summary>
        public int RxBuffSize { get; set; }

        /// <summary>Создаёт новый сокет, который будет зарегистрирован в порту и возвращён функцией OpenSocket</summary>
        protected override ICanSocket CreateSocket()
        {
            return
                new SocketCanSocket(
                    string.Format("Linux сокет на {0}", _interfaceName),
                    new DirectSocketCanReader(_linuxSocketFactory.OpenLinuxSocket(_interfaceName, RxBuffSize, 0)),
                    new IndependentSocketCanWriter(_linuxSocketFactory.OpenLinuxSocket(_interfaceName, TxBuffSize * 3, TxBuffSize)));
        }
    }
}
