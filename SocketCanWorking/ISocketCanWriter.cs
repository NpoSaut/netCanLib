using System.Collections.Generic;
using Communications.Can;

namespace SocketCanWorking
{
    /// <summary>Инструмент по отправке сообщений в сокет</summary>
    public interface ISocketCanWriter
    {
        /// <summary>Выполняет блокирующую отправку сообщений в линию</summary>
        void Send(IList<CanFrame> Frames);
    }
}
