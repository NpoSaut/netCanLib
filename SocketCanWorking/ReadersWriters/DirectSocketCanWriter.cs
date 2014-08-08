using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Can;
using SocketCanWorking.LinuxSockets;

namespace SocketCanWorking.ReadersWriters
{
    public class DirectSocketCanWriter : ISocketCanWriter
    {
        private readonly ILinuxSocket _linuxSocket;
        public DirectSocketCanWriter(ILinuxSocket LinuxSocket) { _linuxSocket = LinuxSocket; }

        /// <summary>Выполняет блокирующую отправку сообщений в линию</summary>
        public void Send(IList<CanFrame> Frames)
        {
            if (Frames.Count < _linuxSocket.TxBufferSize) _linuxSocket.Send(Frames);
            else
            {
                int pointer = 0;
                while (pointer < Frames.Count)
                {
                    int count = Math.Min(_linuxSocket.TxBufferSize, Frames.Count - pointer);
                    _linuxSocket.Send(Frames.Skip(pointer).Take(count).ToList());
                    pointer += count;
                }
            }
        }
    }
}
