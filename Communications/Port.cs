using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public String Name { get; set; }

        protected Port(String Name)
        {
            this.Name = Name;
        }
    }
}
