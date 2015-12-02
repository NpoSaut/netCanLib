using System;

namespace Communications.Options
{
    /// <summary>Опции порта</summary>
    public interface IPortOptions<TFrame>
    {
        /// <summary>Показывает, пересылает ли порт Loopback-пакеты</summary>
        Boolean ProducesLoopback { get; }

        /// <summary>Loopback-инспектор, позволяющий проверить, является ли один пакет Loopback-пакетом для другого</summary>
        ILoopbackInspector<TFrame> LoopbackInspector { get; }
    }
}