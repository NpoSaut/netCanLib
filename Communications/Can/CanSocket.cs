using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Sockets;

namespace Communications.Can
{
    /// <summary>
    /// Базовый класс для CAN-сокетов. Предоставляет функционал по фильтрации фреймов по дескрипторам на входе
    /// </summary>
    public class CanSocket : BufferedFilteredSocketBase<CanFrame>, ICanSocket
    {
        /// <summary>Используемый фильтр</summary>
        public HashSet<int> Filter { get; private set; }

        public CanSocket(string Name, IList<int> Filter) : base(Name)
        {
            if (Filter != null && Filter.Any())
                this.Filter = new HashSet<int>(Filter);
        }
        public CanSocket(string Name, params int[] Filter) : this(Name, (IList<int>)Filter) { }

        protected override bool CheckDatagramBeforeEnqueue(CanFrame Frame)
        {
            return Filter == null || Filter.Contains(Frame.Descriptor);
        }
    }
}