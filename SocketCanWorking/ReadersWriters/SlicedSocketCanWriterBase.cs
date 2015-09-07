using System.Collections.Generic;
using System.Linq;
using Communications.Can;

namespace SocketCanWorking.ReadersWriters
{
    /// <summary>
    ///     Писатель, который отслеживает количество сообщений, поставленых на передачу и нарезающих массив отправляемых
    ///     сообщений на порции
    /// </summary>
    public abstract class SlicedSocketCanWriterBase : ISocketCanWriter
    {
        /// <summary>Выполняет блокирующую отправку сообщений в линию</summary>
        public void Send(IList<CanFrame> Frames)
        {
            int pointer = 0;
            while (pointer < Frames.Count)
            {
                IList<CanFrame> framesToPush =
                    pointer == 0
                        ? Frames
                        : Frames.Skip(pointer).ToList();
                pointer += Push(framesToPush);
            }
        }

        /// <summary>Ставит сообщения на отправку</summary>
        /// <param name="Frames">Список сообщений на отправку</param>
        /// <returns>Количество сообщений из списка, поставленных на отправку</returns>
        protected abstract int Push(IList<CanFrame> Frames);
    }
}
