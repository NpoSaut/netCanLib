using System;
using Communications.Options;
using Communications.Transactions;

namespace Communications
{
    /// <summary>Порт</summary>
    /// <typeparam name="TFrame">Тип кадра, транслируемого через порт</typeparam>
    /// <typeparam name="TOptions">Тип опций порта</typeparam>
    public interface IPort<TFrame, out TOptions> : IDisposable
        where TOptions : IPortOptions<TFrame>
    {
        /// <summary>Поток входящих сообщений</summary>
        IObservable<ITransaction<TFrame>> Rx { get; }

        /// <summary>Поток исходящих сообщений</summary>
        [Obsolete("Нужно использовать функцию BeginSend()", true)]
        IObserver<TFrame> Tx { get; }

        /// <summary>Опции порта</summary>
        TOptions Options { get; }

        /// <summary>Начинает отправку кадра</summary>
        /// <param name="Frame">Кадр для отправки</param>
        /// <returns>Транзакция передачи</returns>
        ITransaction<TFrame> BeginSend(TFrame Frame);
    }

    /// <summary>Порт с опциями по-умолчанию</summary>
    /// <typeparam name="TFrame">Тип кадра, транслируемого через порт</typeparam>
    public interface IPort<TFrame> : IPort<TFrame, IPortOptions<TFrame>> { }
}
