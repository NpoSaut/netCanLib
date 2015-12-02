namespace Communications.Appi.Buffers
{
    /// <summary>Буфер АППИ</summary>
    public abstract class Buffer
    {
        protected Buffer(int SequentialNumber) { this.SequentialNumber = SequentialNumber; }

        /// <summary>Сквозной номер буфера</summary>
        public int SequentialNumber { get; private set; }
    }
}
