using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Serial
{
    /// <summary>
    /// Абстрактный класс, представляющий работу с последовательными портами
    /// </summary>
    public abstract class RsPort : PipedPortBase<ISocket<Byte>, Byte>
    {
        /// <summary>
        /// Создаёт экземпляр порта, работающего с нижнем уровнем посредством труб
        /// </summary>
        /// <param name="Name">Имя порта</param>
        /// <param name="SendPipe">Труба, в которую будут переданы отправляемые сообщения из сокетов</param>
        /// <param name="ReceivePipe">Труба, из которой ожидаются входящие сообщения</param>
        protected RsPort(string Name, ISendPipe<byte> SendPipe, IReceivePipe<byte> ReceivePipe) : base(Name, SendPipe, ReceivePipe) { }
    }
}
