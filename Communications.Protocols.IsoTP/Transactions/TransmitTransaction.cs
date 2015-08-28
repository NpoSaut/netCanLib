using System;
using System.IO;
using Communications.Transactions;

namespace Communications.Protocols.IsoTP.Transactions
{
    public class TransmitTransaction : LongTransactionBase<IsoTpPacket>
    {
        private readonly IsoTpPacket _packet;
        private readonly BinaryReader _reader;
        private readonly MemoryStream _stream;

        public TransmitTransaction(IsoTpPacket Packet)
        {
            _packet = Packet;
            _stream = new MemoryStream(Packet.Data);
            _reader = new BinaryReader(_stream);
            Index = 1;
        }

        public override bool Done
        {
            get { return _stream.Position >= _stream.Length; }
        }

        public int Index { get; private set; }

        public long Length
        {
            get { return _stream.Length; }
        }

        protected override IsoTpPacket GetPayload() { return _packet; }

        public byte[] GetDataSlice(long Size) { return _reader.ReadBytes((int)Math.Min(Size, _stream.Length - _stream.Position)); }

        public void IncreaseIndex() { Index = (byte)(Index + 1); }

        public override string ToString() { return string.Format("ISO-TP Transaction [Done: {0}, Index: {1}, Length: {2} Bytes]", Done, Index, Length); }
    }
}
