using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Communications.Can
{
    /// <summary>
    /// Абстракция CAN-порта
    /// </summary>
    public abstract class CanPort : PortBase<ICanSocket, CanFrame>
    {
        protected CanPort(string Name) : base(Name) { }
    }
}
