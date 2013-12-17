using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Communications.Can
{
    /// <summary>
    /// Абстракция CAN-порта
    /// </summary>
    public abstract class CanPort : PortBase<ICanSocket, CanFrame>, ICanSocketSource
    {
        protected CanPort(string Name) : base(Name) { }

        /// <summary>
        /// Открывает CAN-сокет, способный отфильтровывать на входе все фреймы с дескрипторами, не указанными в фильтре
        /// </summary>
        /// <param name="FilterDescriptors">Принимаемые дескрипторы. Остальные будут отфильтрованы</param>
        public ICanSocket OpenSocket(params int[] FilterDescriptors)
        {
            var res = base.OpenSocket();
            if (FilterDescriptors.Any()) res.Filter = new HashSet<int>(FilterDescriptors);
            return res;
        }
    }
}
