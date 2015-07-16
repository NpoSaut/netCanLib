using System;

namespace Communications
{
    /// <summary>Пакет с бинарными данными</summary>
    public interface IDataFrame
    {
        /// <summary>Данные, содержащиеся в пакете</summary>
        Byte[] Data { get; }
    }
}