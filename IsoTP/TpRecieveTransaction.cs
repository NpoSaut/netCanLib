using System;
using System.IO;

namespace Communications.Protocols.IsoTP
{
    public class TpReceiveTransaction : TpTransaction
    {
        public Byte[] Data { get; private set; }
        public MemoryStream DataStream { get; private set; }

        public bool Done
        {
            get { return DataStream.Position == DataStream.Length - 1; }
        }

        public byte ExpectedFrameIndex { get; set; }

        public TpReceiveTransaction(int Length) : base(Length)
        {
            Data = new byte[Length];
            DataStream = new MemoryStream(Data);
        }
        public void Write(byte[] Bytes) { DataStream.Write(Bytes, 0, Bytes.Length); }
    }
}
