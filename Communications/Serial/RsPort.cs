using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications.Serial
{
    /// <summary>
    /// Абстрактный класс, представляющий работу с последовательными портами
    /// </summary>
    public abstract class RsPort : PortBase<ISocket<Byte>, Byte>
    {
        protected RsPort(String Name)
            : base(Name)
        { }
    }
}
