using System;
using System.Threading;

namespace Communications.Transactions
{
    /// <summary>Транзакция</summary>
    public interface ITransaction<out TPayload>
    {
        /// <summary>Содержимое транзакции</summary>
        TPayload Payload { get; }

        /// <summary>Показывает, завершена ли транзакция</summary>
        bool Done { get; }

        /// <summary>Ожидает завершения транзакции</summary>
        /// <param name="Timeout">Таймаут ожидания завершения транзакции</param>
        /// <param name="CancellationToken">Токен для отмены транзакции</param>
        /// <returns>Содержимое транзакции</returns>
        TPayload Wait(TimeSpan Timeout, CancellationToken CancellationToken);
    }
}
