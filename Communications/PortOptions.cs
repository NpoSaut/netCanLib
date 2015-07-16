using System;

namespace Communications
{
    /// <summary>Опции порта</summary>
    public class PortOptions<TFrame>
    {
        /// <summary>Создаёт новые опции порта без поддержки Loopback</summary>
        public PortOptions()
        {
            ProducesLoopback = false;
            LoopbackInspector = new LambdaLoopbackInspector<TFrame>((a, b) => false);
        }

        /// <summary>Создаёт новые опции порта с поддержкой Loopback и указанным <see cref="ILoopbackInspector{TFrame}" />
        /// </summary>
        /// <param name="LoopbackInspector">Инструмент проверки на Loopback-пакет</param>
        public PortOptions(ILoopbackInspector<TFrame> LoopbackInspector)
        {
            ProducesLoopback = true;
            this.LoopbackInspector = LoopbackInspector;
        }

        /// <summary>Показывает, пересылает ли порт Loopback-пакеты</summary>
        public Boolean ProducesLoopback { get; private set; }

        /// <summary>Loopback-инспектор, позволяющий проверить, является ли один пакет Loopback-пакетом для другого</summary>
        public ILoopbackInspector<TFrame> LoopbackInspector { get; private set; }
    }
}
