using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Serial
{
    /// <summary>
    /// Абстрактный класс, представляющий работу с последовательными портами
    /// </summary>
    public abstract class RsPort : Port
    {
        protected abstract Byte[] ReadBufferImplementation();
        protected abstract void WriteBufferImplementation(Byte[] buff);

        protected RsPort(String Name)
            : base(Name)
        { }

        /// <summary>
        /// Прочитывает входящий поток до конца
        /// </summary>
        /// <returns>Прочитанные данные</returns>
        public Byte[] ReadAll()
        {
            return ReadBufferImplementation();
        }
        /// <summary>
        /// Отправляет буфер
        /// </summary>
        /// <param name="buff">Буфер с данными для отправки</param>
        public void Write(Byte[] buff)
        {
            if (buff.Length > 0)
                WriteBufferImplementation(buff);
        }

        [Obsolete("Работа с потоком ещё не реализована", true)]
        public RsStream OpenStream()
        {
            return new RsStream(this);
        }
    }
}
