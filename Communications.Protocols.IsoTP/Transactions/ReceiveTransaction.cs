﻿using System.IO;
using Communications.Protocols.IsoTP.Exceptions;

namespace Communications.Protocols.IsoTP.Transactions
{
    public class ReceiveTransaction
    {
        private readonly int _packetSize;
        private readonly MemoryStream _stream;
        private readonly BinaryWriter _writer;

        public ReceiveTransaction(int PacketSize)
        {
            _packetSize = PacketSize;
            _stream = new MemoryStream(PacketSize);
            _writer = new BinaryWriter(_stream);
        }

        public int ExpectedCounter { get; private set; }
        public int BlockCounter { get; set; }

        public bool Done
        {
            get { return _writer.BaseStream.Position >= _packetSize; }
        }

        public void PushDataSlice(byte[] Data)
        {
            _writer.Write(Data);
            ExpectedCounter = (byte)(ExpectedCounter + 1);
        }

        public IsoTpPacket GetPacket()
        {
            if (!Done)
                throw new IsoTpProtocolException("Попытались извлечь пакет из не законченной транзакции");
            return new IsoTpPacket(_stream.ToArray());
        }
    }
}