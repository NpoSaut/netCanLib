﻿using System;
using System.IO;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.ReceiveStates
{
    public class IsoTpReceiveTransactionContext
    {
        private readonly MemoryStream _dataStream;

        private readonly int _packetSize;

        public IsoTpReceiveTransactionContext(int PacketSize, IObserver<IsoTpPacket> Observer, IObserver<IsoTpFrame> Tx, byte BlockSize, TimeSpan SeparationTime, TimeSpan Timeout)
        {
            _packetSize = PacketSize;
            this.Timeout = Timeout;
            this.Observer = Observer;
            this.Tx = Tx;
            this.BlockSize = BlockSize;
            this.SeparationTime = SeparationTime;
            _dataStream = new MemoryStream();
            ExpectedFrameIndex = 1;
        }

        public IObserver<IsoTpPacket> Observer { get; private set; }
        public IObserver<IsoTpFrame> Tx { get; private set; }
        public byte BlockSize { get; private set; }
        public TimeSpan SeparationTime { get; private set; }
        public byte ExpectedFrameIndex { get; private set; }

        public bool IsDone
        {
            get { return _dataStream.Position == _packetSize; }
        }

        public TimeSpan Timeout { get; private set; }

        public void Write(byte[] Data)
        {
            _dataStream.Write(Data, 0, Data.Length);
        }

        public void IncreaseFrameIndex() { ExpectedFrameIndex = (byte)((ExpectedFrameIndex + 1) & 0x0f); }

        public void Submit()
        {
            Observer.OnNext(new IsoTpPacket(_dataStream.ToArray()));
        }

        public void OnError(Exception e) { Observer.OnError(e); }

        /// <summary>Возвращает объект <see cref="T:System.String" />, который представляет текущий объект
        ///     <see cref="T:System.Object" />.</summary>
        /// <returns>Объект <see cref="T:System.String" />, представляющий текущий объект <see cref="T:System.Object" />.</returns>
        public override string ToString()
        {
            return string.Format("RECEIVE  {4}/{5} IsDone: {0}, ExpectedFrameIndex: {1}, BlockSize: {2}, SeparationTime: {3}", IsDone, ExpectedFrameIndex,
                                 BlockSize, SeparationTime, _dataStream.Position, _packetSize);
        }
    }
}
