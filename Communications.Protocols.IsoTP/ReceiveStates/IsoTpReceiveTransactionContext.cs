using System;
using System.IO;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.ReceiveStates
{
    public class IsoTpReceiveTransactionContext
    {
        private readonly MemoryStream _dataStream;

        private readonly int _packetSize;

        public IsoTpReceiveTransactionContext(int PacketSize, IObserver<IsoTpPacket> Observer, IObserver<IsoTpFrame> Tx,
                                              IsoTpConnectionParameters ConnectionParameters)
        {
            _packetSize = PacketSize;
            this.ConnectionParameters = ConnectionParameters;
            this.Observer = Observer;
            this.Tx = Tx;
            _dataStream = new MemoryStream();
            ExpectedFrameIndex = 1;
        }

        public IsoTpConnectionParameters ConnectionParameters { get; private set; }
        public IObserver<IsoTpPacket> Observer { get; private set; }
        public IObserver<IsoTpFrame> Tx { get; private set; }
        public byte ExpectedFrameIndex { get; private set; }

        public bool IsDone
        {
            get { return _dataStream.Position == _packetSize; }
        }

        public void Write(byte[] Data) { _dataStream.Write(Data, 0, Data.Length); }

        public void IncreaseFrameIndex() { ExpectedFrameIndex = (byte)((ExpectedFrameIndex + 1) & 0x0f); }

        public void Submit() { Observer.OnNext(new IsoTpPacket(_dataStream.ToArray())); }

        public void OnError(Exception e) { Observer.OnError(e); }

        /// <summary>Возвращает объект <see cref="T:System.String" />, который представляет текущий объект
        ///     <see cref="T:System.Object" />.</summary>
        /// <returns>Объект <see cref="T:System.String" />, представляющий текущий объект <see cref="T:System.Object" />.</returns>
        public override string ToString()
        {
            return string.Format("RECEIVE  {3}/{4} IsDone: {0}, ExpectedFrameIndex: {1}, Parameters: {2}", IsDone, ExpectedFrameIndex, ConnectionParameters, _dataStream.Position, _packetSize);
        }
    }
}
