using System;
using System.Collections.Generic;
using Communications.Can;
using SocketCanWorking.LinuxSockets;

namespace SocketCanWorking.ReadersWriters
{
    public class DirectSocketCanReader : ISocketCanReader
    {
        private readonly ILinuxSocket _linuxSocket;
        public DirectSocketCanReader(ILinuxSocket LinuxSocket) { _linuxSocket = LinuxSocket; }

        /// <summary>Выполняет блокирующее чтение из сокета</summary>
        /// <param name="Timeout">Таймаут чтения</param>
        public IEnumerable<CanFrame> Receive(TimeSpan Timeout) { return _linuxSocket.Receive(Timeout); }
    }
}
