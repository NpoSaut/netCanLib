using System.Collections.Generic;
using System.Linq;
using Communications.Can;

namespace SocketCanWorking.ReadersWriters
{
    internal class CanFrameComparer : IEqualityComparer<CanFrame>
    {
        public bool Equals(CanFrame x, CanFrame y)
        {
            return x.Id == y.Id &&
                   x.Data.SequenceEqual(y.Data);
        }

        public int GetHashCode(CanFrame obj) { return obj.Id ^ obj.Data.Aggregate(0, (hash, b) => (hash << 3) | (hash >> (29)) ^ b); }
    }
}
