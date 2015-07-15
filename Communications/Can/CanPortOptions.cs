namespace Communications.Can
{
    /// <summary>Опции CAN-порта</summary>
    public class CanPortOptions : PortOptions<CanFrame>
    {
        /// <summary>Скорость обмена</summary>
        public int BaudRate { get; private set; }
    }
}