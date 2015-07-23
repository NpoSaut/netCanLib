using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Communications.Protocols.IsoTP.Transactions
{
    public class TransmitTransaction
    {
        private readonly BinaryReader _reader;

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly MemoryStream _stream;
        private Exception _transactionException;

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

        public long Length
        {
            get { return _stream.Length; }
        }

        public byte[] GetDataSlice(long Size) { return _reader.ReadBytes((int)Math.Min(Size, _stream.Length - _stream.Position)); }

        public void IncreaseIndex() { Index = (byte)(Index + 1); }

        public void Submit()
        {
            _resetEvent.Set();
        }

        public void Fail(Exception Exception)
        {
            _transactionException = Exception;
            _resetEvent.Set();
        }

        public void Wait()
        {
            _resetEvent.WaitOne();
            if (_transactionException != null)
                throw _transactionException;
        }
    }
}
