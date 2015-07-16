namespace Communications
{
    /// <summary>Опции порта передачи бинарных данных</summary>
    public interface IDataPortOptions
    {
        /// <summary>Максимальная вместимость поля <see cref="IDataFrame.Data"/> одного пакета (в единицах байт)</summary>
        int DataCapacity { get; }
    }
}