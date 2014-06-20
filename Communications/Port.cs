using System;

namespace Communications
{
    /// <summary>Представляет логику абстрактного порта</summary>
    public abstract class Port
    {
        protected Port(String Name) { this.Name = Name; }

        /// <summary>Имя порта</summary>
        public String Name { get; private set; }

        /// <summary>Событие, оповещающее о том, что порт был закрыт</summary>
        public event EventHandler Closed;

        /// <summary>Выстреливать при закрытии порта</summary>
        protected virtual void OnClosed()
        {
            EventHandler handler = Closed;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
