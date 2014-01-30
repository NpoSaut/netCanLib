using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications
{
    public interface IClosable : IDisposable
    {
        /// <summary>Показывает, что сокет не был закрыт.</summary>
        /// <remarks>Сокет открывается однажды при создании и закрывается однажды и навсегда.</remarks>
        bool IsOpened { get; }
        /// <summary>Событие, возникающее при уничтожении (закрытии) сокета</summary>
        event EventHandler Closed;
    }
}
