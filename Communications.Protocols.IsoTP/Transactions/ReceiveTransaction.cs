using System;
using System.IO;
using Communications.Transactions;

namespace Communications.Protocols.IsoTP.Transactions
{
    public class ReceiveTransaction : LongTransactionBase<IsoTpPacket>
    {
        private readonly int _packetSize;
        private readonly MemoryStream _stream;
        private readonly BinaryWriter _writer;

        public ReceiveTransaction(int PacketSize, int BlockSize)
        {
            _packetSize = PacketSize;
            _stream = new MemoryStream(PacketSize);
            _writer = new BinaryWriter(_stream);
            BlockCounter = BlockSize;
        }

        public int ExpectedCounter { get; private set; }
        public int BlockCounter { get; set; }

        public bool AllDataReceived
        {
            get { return _writer.BaseStream.Position >= _packetSize; }
        }

        protected override IsoTpPacket GetPayload() { return new IsoTpPacket(_stream.ToArray()); }

        public void PushDataSlice(byte[] Data)
        {
            _writer.Write(Data, 0, Math.Min(Data.Length, _packetSize - (int)_stream.Position));
            ExpectedCounter = 0x0f & (ExpectedCounter + 1);
        }
    }
}
