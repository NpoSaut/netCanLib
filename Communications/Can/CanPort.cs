using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Communications.Can
{
    /// <summary>
    /// Абстракция CAN-порта
    /// </summary>
    public class CanPort : PipedPortBase<ICanSocket, CanFrame>
    {
        /// <summary>
        /// Создаёт экземпляр порта CAN
        /// </summary>
        /// <param name="Name">Имя порта</param>
        /// <param name="SendPipe">Труба, в которую будут переданы отправляемые сообщения из сокетов</param>
        /// <param name="ReceivePipe">Труба, из которой ожидаются входящие сообщения</param>
        public CanPort(string Name, ISendPipe<CanFrame> SendPipe, IReceivePipe<CanFrame> ReceivePipe) : base(Name, SendPipe, ReceivePipe) { }

        public int BaudRate { get; set; }
        public int SamplePoint { get; set; }

        /// <summary>
        /// Открывает CAN-сокет, способный отфильтровывать на входе все фреймы с дескрипторами, не указанными в фильтре
        /// </summary>
        /// <param name="FilterDescriptors">Принимаемые дескрипторы. Остальные будут отфильтрованы</param>
        public ICanSocket OpenSocket(params int[] FilterDescriptors)
        {
            var socket = new CanSocket(Name, FilterDescriptors);
            RegisterSocketBackend(socket);
            return socket;
        }

        /// <summary>Открывает новый сокет</summary>
        public override ICanSocket OpenSocket() { return OpenFilteredSocket(); }

        /// <summary>Открывает новый CAN-сокет с фильтром входящих сообщений по дескриптору</summary>
        /// <param name="Filter">Список принимаемых дескрипторов</param>
        public ICanSocket OpenFilteredSocket(params int[] Filter)
        {
            var socket = new CanSocket(string.Format("CAN Socket on {0}", Name));
            RegisterSocketBackend(socket);
            return socket;
        }
    }
}
