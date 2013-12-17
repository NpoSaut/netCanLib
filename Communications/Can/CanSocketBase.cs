using System;
using System.Collections.Generic;
using Communications.Sockets;

namespace Communications.Can
{
    /// <summary>
    /// Базовый класс для CAN-сокетов. Предоставляет функционал по фильтрации фреймов по дескрипторам на входе
    /// </summary>
    public abstract class CanSocketBase : BufferedSocketBase<CanFrame>, ICanSocket
    {
        public HashSet<int> Filter { get; set; }

        protected CanSocketBase(string Name) : base(Name) { }

        protected override bool CheckDatagramBeforeEnqueue(CanFrame Frame) { return Filter == null || Filter.Contains(Frame.Descriptor); }
    }
}