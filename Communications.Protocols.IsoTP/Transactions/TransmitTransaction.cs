using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Communications.Protocols.IsoTP.Transactions
{
    public class TransmitTransaction
    {
        private readonly BinaryReader _reader;
        private readonly MemoryStream _stream;

        private AutoResetEvent _resetEvent = new AutoResetEvent(false);

        public TransmitTransaction(byte[] Data)
        {
            _stream = new MemoryStream(Data);
            _reader = new BinaryReader(_stream);
            Index = 1;
        }

        public bool Done
        {
            get { return _stream.Position >= _stream.Length; }
        }

        public int Index { get; private set; }

        public byte[] GetDataSlice(int Size)
        {
            return _reader.ReadBytes((int)Math.Min(Size, _stream.Length - _stream.Position));
        }

        public void IncreaseIndex() { Index = (byte)(Index + 1); }
        public void Submit() { _resetEvent.Reset(); }
        public void Wait() { _resetEvent.WaitOne(); }
    }
}
