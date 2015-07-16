namespace Communications
{
    /// <summary>Опции порта передачи бинарных данных по-умолчанию</summary>
    /// <typeparam name="TFrame">Тип кадра</typeparam>
    public class DataPortOptions<TFrame> : PortOptions<TFrame>, IDataPortOptions
        where TFrame : IDataFrame
    {
        /// <summary>Создаёт новые опции порта без поддержки Loopback</summary>
        /// <param name="DataCapacity">Максимальная вместимость одного пакета</param>
        public DataPortOptions(int DataCapacity) { this.DataCapacity = DataCapacity; }

        /// <summary>Создаёт новые опции порта с поддержкой Loopback и указанным <see cref="ILoopbackInspector{TFrame}" />
        /// </summary>
        /// <param name="LoopbackInspector">Инструмент проверки на Loopback-пакет</param>
        /// <param name="DataCapacity">Максимальная вместимость одного пакета</param>
        public DataPortOptions(ILoopbackInspector<TFrame> LoopbackInspector, int DataCapacity) : base(LoopbackInspector) { this.DataCapacity = DataCapacity; }

        /// <summary>Максимальная вместимость поля <see cref="IDataFrame.Data" /> одного пакета (в единицах байт)</summary>
        public int DataCapacity { get; private set; }
    }
}
