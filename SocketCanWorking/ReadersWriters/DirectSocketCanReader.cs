using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Communications.Can;
using Communications.Exceptions;
using SocketCanWorking.LinuxSockets;

namespace SocketCanWorking.ReadersWriters
{
    public class DirectSocketCanReader : ISocketCanReader
    {
        private readonly ILinuxSocket _linuxSocket;
        public DirectSocketCanReader(ILinuxSocket LinuxSocket) { _linuxSocket = LinuxSocket; }

        /// <summary>Выполняет блокирующее чтение из сокета</summary>
        /// <param name="Timeout">Таймаут чтения</param>
        public IEnumerable<CanFrame> Receive(TimeSpan Timeout)
        {
            // Засекаем время
            var sw = new Stopwatch();
            sw.Start();

            // Продолжаем загребать из буфера, пока не выйдет таймаут
            while (sw.Elapsed <= Timeout)
            {
                IList<CanFrame> frames = _linuxSocket.Receive(Timeout - sw.Elapsed);
                foreach (CanFrame frame in frames) yield return frame;
                // Если получено хоть одно сообщение, сбрасываем таймер
                if (frames.Any()) sw.Restart();
            }
            throw new SocketReadTimeoutException();
        }
    }
}
