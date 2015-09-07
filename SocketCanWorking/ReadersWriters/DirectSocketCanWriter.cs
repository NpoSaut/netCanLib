using System.Collections.Generic;
using Communications.Can;
using SocketCanWorking.LinuxSockets;

namespace SocketCanWorking.ReadersWriters
{
    public class DirectSocketCanWriter : SlicedSocketCanWriterBase
    {
        private readonly ILinuxSocket _linuxSocket;
        public DirectSocketCanWriter(ILinuxSocket LinuxSocket) { _linuxSocket = LinuxSocket; }

        /// <summary>Ставит сообщения на отправку</summary>
        /// <param name="Frames">Список сообщений на отправку</param>
        /// <returns>Количество сообщений из списка, поставленных на отправку</returns>
        protected override int Push(IList<CanFrame> Frames) { return _linuxSocket.Send(Frames); }
    }
}
