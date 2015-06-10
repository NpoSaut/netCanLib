using System;

namespace Communications.Protocols.IsoTP
{
    public class TpReceiveTransaction : TpTransaction
    {
        public TpReceiveTransaction(int Length) : base(new byte[Length]) { }

        public byte ExpectedFrameIndex { get; set; }

        public void Write(byte[] Bytes) { DataStream.Write(Bytes, 0, Math.Min(Bytes.Length, (int)(DataStream.Length - DataStream.Position))); }
    }
}
