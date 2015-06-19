using System;
using System.IO;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.Contexts
{
    public class IsoTpTransmitTransactionContext
    {
        private readonly BinaryReader _streamReader;

        public IsoTpTransmitTransactionContext(IsoTpPacket Packet, int FrameLayerCapacity, IObserver<IsoTpFrame> Tx)
        {
            this.FrameLayerCapacity = FrameLayerCapacity;
            this.Tx = Tx;
            this.Packet = Packet;
            _streamReader = new BinaryReader(new MemoryStream(Packet.Data));
        }

        public IsoTpPacket Packet { get; private set; }
        public int FrameLayerCapacity { get; set; }
        public IObserver<IsoTpFrame> Tx { get; set; }

        public int PacketLength
        {
            get { return (int)_streamReader.BaseStream.Length; }
        }

        public bool IsReady
        {
            get { return _streamReader.BaseStream.Position == _streamReader.BaseStream.Length; }
        }

        public byte Index { get; set; }
        public bool IsAborted { get; set; }
        public TimeSpan Timeout { get; set; }

        public byte[] Read(int Length) { return _streamReader.ReadBytes(Length); }
        public void Submit() { }

        /// <summary>
        /// Возвращает объект <see cref="T:System.String"/>, который представляет текущий объект <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.String"/>, представляющий текущий объект <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("TRANSMIT {0} / {1}, IsReady: {2}, IsAborted: {3}, Index: {4}", _streamReader.BaseStream.Position, PacketLength, IsReady, IsAborted, Index);
        }
    }
}
