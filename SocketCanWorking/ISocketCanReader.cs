using System;
using System.Collections.Generic;
using Communications.Can;

namespace SocketCanWorking
{
    /// <summary>Инструмент по чтению сообщений из сокета</summary>
    public interface ISocketCanReader
    {
        /// <summary>Выполняет блокирующее чтение из сокета</summary>
        /// <param name="Timeout">Таймаут чтения</param>
        IEnumerable<CanFrame> Receive(TimeSpan Timeout);
    }
}
