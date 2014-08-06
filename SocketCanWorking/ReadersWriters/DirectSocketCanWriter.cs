using System.Collections.Generic;
using Communications.Can;
using SocketCanWorking.LinuxSockets;

namespace SocketCanWorking.ReadersWriters
{
    public class DirectSocketCanWriter : ISocketCanWriter
    {
        private readonly ILinuxSocket _linuxSocket;
        public DirectSocketCanWriter(ILinuxSocket LinuxSocket) { _linuxSocket = LinuxSocket; }

        /// <summary>Выполняет блокирующую отправку сообщений в линию</summary>
        public void Send(IList<CanFrame> Frames) { _linuxSocket.Send(Frames); }
    }
}
