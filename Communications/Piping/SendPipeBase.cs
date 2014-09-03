using System;
using System.Collections.Generic;

namespace Communications.Piping
{
    /// <summary>Реализует базовую абстракцию для <see cref="ISendPipe{TDatagram}" />
    /// </summary>
    /// <typeparam name="TDatagram">Тип дейтаграммы</typeparam>
    public abstract class SendPipeBase<TDatagram> : ISendPipe<TDatagram>
    {
        /// <summary>Передаёт дейтаграммы на низлежащий уровень для его отправки с максимально возможным таймаутом</summary>
        /// <param name="Datagrams">Кадры для отправки</param>
        public virtual void Send(IList<TDatagram> Datagrams) { Send(Datagrams, TimeSpan.MaxValue); }

        /// <summary>Передаёт дейтаграммы на низлежащий уровень для его отправки</summary>
        /// <param name="Datagrams">Кадры для отправки</param>
        /// <param name="Timeout">Таймаут операции</param>
        public abstract void Send(IList<TDatagram> Datagrams, TimeSpan Timeout);
    }
}
