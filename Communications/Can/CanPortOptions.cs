using Communications.Options;

namespace Communications.Can
{
    /// <summary>Опции CAN-порта</summary>
    public abstract class CanPortOptions : PortOptions<CanFrame>, ICanPortOptions
    {
        /// <summary>Создаёт новые опции порта без поддержки Loopback</summary>
        protected CanPortOptions() { }

        /// <summary>Создаёт новые опции порта с поддержкой Loopback и указанным <see cref="ILoopbackInspector{TFrame}" />
        /// </summary>
        /// <param name="LoopbackInspector">Инструмент проверки на Loopback-пакет</param>
        protected CanPortOptions(ILoopbackInspector<CanFrame> LoopbackInspector) : base(LoopbackInspector) { }

        /// <summary>Максимальная вместимость поля <see cref="IDataFrame.Data" /> одного пакета (в единицах байт)</summary>
        public int DataCapacity
        {
            get { return 8; }
        }

        /// <summary>Скорость обмена</summary>
        public abstract int BaudRate { get; set; }
    }
}
