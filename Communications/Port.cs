using System;

namespace Communications
{
    /// <summary>
    /// Представляет логику абстрактного порта
    /// </summary>
    public abstract class Port
    {
        /// <summary>
        /// Имя порта
        /// </summary>
        public String Name { get; private set; }

        protected Port(String Name)
        {
            this.Name = Name;
        }
    }
}
