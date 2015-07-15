using System;

namespace Communications
{
    /// <summary>Порт</summary>
    /// <typeparam name="TFrame">Тип кадра, транслируемого через порт</typeparam>
    /// <typeparam name="TOptions">Тип опций порта</typeparam>
    public interface IPort<TFrame, out TOptions> : IDisposable where TOptions : PortOptions<TFrame>
    {
        /// <summary>Поток входящих сообщений</summary>
        IObservable<TFrame> Rx { get; }

        /// <summary>Поток исходящих сообщений</summary>
        IObserver<TFrame> Tx { get; }

        /// <summary>Опции порта</summary>
        TOptions Options { get; }
    }

    /// <summary>Порт с опциями по-умолчанию</summary>
    /// <typeparam name="TFrame">Тип кадра, транслируемого через порт</typeparam>
    public interface IPort<TFrame> : IPort<TFrame, PortOptions<TFrame>> { }
}
