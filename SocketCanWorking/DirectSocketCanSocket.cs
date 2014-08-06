using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Can;
using Communications.Sockets;

namespace SocketCanWorking
{
    /// <summary>Сокет, напрямую реализующий работы с Linux SocketCan</summary>
    public class DirectSocketCanSocket : SocketBase<CanFrame>, ICanSocket
    {
        private readonly ILinuxSocket _linuxSocket;

        public DirectSocketCanSocket(string Name, ILinuxSocket LinuxSocket) : base(Name) { _linuxSocket = LinuxSocket; }

        protected override void ImplementSend(IEnumerable<CanFrame> Datagrams) { _linuxSocket.Send(Datagrams.ToList()); }
        protected override IEnumerable<CanFrame> ImplementReceive(TimeSpan Timeout) { return _linuxSocket.Receive(Timeout); }
    }
}
