using System;

namespace Communications.Protocols.IsoTP
{
    public class TpSendTransaction : TpTransaction
    {
        public TpSendTransaction(Byte[] DataBytes) : base(DataBytes) { }
        public int Index { get; set; }
        public int BlockSize { get; set; }
        public TimeSpan SeparationTime { get; set; }

        public Byte[] GetBytes(int Count)
        {
            Count = Math.Min(Count, Length - Position);
            var res = new byte[Count];
            DataStream.Read(res, 0, Count);
            return res;
        }
    }
}
