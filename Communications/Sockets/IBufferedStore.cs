using System.Collections.Generic;

namespace Communications.Sockets
{
    public interface IBufferedStore<TDatagram> {
        /// <summary>
        /// Добавляет датаграммы в очередь на обработку
        /// </summary>
        /// <param name="Datagrams">Полученные датаграммы</param>
        void Enqueue(IEnumerable<TDatagram> Datagrams);
    }
}