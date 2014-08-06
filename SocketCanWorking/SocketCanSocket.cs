using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Can;
using Communications.Sockets;

namespace SocketCanWorking
{
    public class SocketCanSocket : SocketBase<CanFrame>, ICanSocket
    {
        private readonly ISocketCanReader _reader;
        private readonly ISocketCanWriter _writer;

        public SocketCanSocket(string Name, ISocketCanReader Reader, ISocketCanWriter Writer) : base(Name)
        {
            _reader = Reader;
            _writer = Writer;
        }

        protected override IEnumerable<CanFrame> ImplementReceive(TimeSpan Timeout) { return _reader.Receive(Timeout); }
        protected override void ImplementSend(IEnumerable<CanFrame> Datagrams) { _writer.Send(Datagrams.ToList()); }
    }
}
