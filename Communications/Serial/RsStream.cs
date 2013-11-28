using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Communications.Serial
{
    /// <summary>
    /// Представляет последовательный порт для работы в потоковом стиле
    /// </summary>
    [Obsolete("Этот класс ещё не реализован")]
    public class RsStream : Stream
    {
        #region Определение параметров Stream
        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        } 
        #endregion

        public RsPort Port { get; set; }
        private readonly object _incomingBufferLocker = new object();
        private Queue<Byte> IncomingBuffer { get; set; }

        public RsStream(RsPort OnPort)
        {
            this.Port = OnPort;
            IncomingBuffer = new Queue<byte>();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_incomingBufferLocker)
            {
                int len = Math.Min(count, IncomingBuffer.Count);
                for (int i = 0; i < len; i++)
                    buffer[offset + i] = IncomingBuffer.Dequeue();
                return len;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var buff = new Byte[count];
            Buffer.BlockCopy(buffer, offset, buff, 0, buff.Length);
            Port.Write(buff);
        }
    }
}
