namespace Communications.Options
{
    /// <summary>Опции порта, который позволяет настраивать скорость обмена</summary>
    public interface IBaudRatePortOptions
    {
        /// <summary>Скорость обмена</summary>
        int BaudRate { get; set; }
    }
}
