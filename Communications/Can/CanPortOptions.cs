namespace Communications.Can
{
    /// <summary>Опции CAN-порта</summary>
    public class CanPortOptions : PortOptions<CanFrame>, IDataPortOptions
    {
        /// <summary>Скорость обмена</summary>
        public int BaudRate { get; private set; }

        /// <summary>Максимальная вместимость поля <see cref="IDataFrame.Data"/> одного пакета (в единицах байт)</summary>
        public int DataCapacity { get; private set; }
    }
}