using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Can;
using Communications.Piping.Packages;
using SocketCanWorking.LinuxSockets;

namespace SocketCanWorking.ReadersWriters
{
    public class IndependentSocketCanWriter : SlicedSocketCanWriterBase
    {
        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromMilliseconds(0);
        private readonly ILinuxSocket _linuxSocket;
        public IndependentSocketCanWriter(ILinuxSocket LinuxSocket) { _linuxSocket = LinuxSocket; }

        /// <summary>Ставит сообщения на отправку и дожидается их появления во входящем потоке сокета</summary>
        /// <param name="Frames">Список сообщений на отправку</param>
        /// <returns>Количество сообщений из списка, поставленных на отправку</returns>
        protected override int Push(IList<CanFrame> Frames)
        {
            _linuxSocket.FlushInBuffer();
            int pushSize = _linuxSocket.Send(Frames);
            IList<CanFrame> pushedFrames = pushSize == Frames.Count ? Frames : Frames.Take(pushSize).ToList();
            var pack = new DirectiveOutgoingPack<CanFrame>(pushedFrames, new CanFrameComparer());
            while (!pack.IsDone)
            {
                foreach (CanFrame frame in _linuxSocket.Receive(ReceiveTimeout).Where(f => f.IsLoopback))
                    pack.MarkAsSent(frame);
            }
            return pushSize;
        }
    }
}
