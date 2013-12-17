using System.Collections.Generic;

namespace Communications.Can
{
    public interface ICanSocket : ISocket<CanFrame>
    {
        HashSet<int> Filter { get; set; }
    }
}