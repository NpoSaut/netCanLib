namespace Communications
{
    /// <summary>Что-то с возможностью установки скорости обмена данными</summary>
    public interface IBaudRateable
    {
        /// <summary>Скорость обмена данными</summary>
        int BaudRate { get; set; }
    }
}
