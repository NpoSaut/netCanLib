using Communications.Appi.Buffers;

namespace Communications.Appi.Decoders
{
    public abstract class BufferDecoderBase : IAppiBufferDecoder
    {
        private readonly int _sequentialNumberOffset;
        protected BufferDecoderBase(int SequentialNumberOffset) { _sequentialNumberOffset = SequentialNumberOffset; }
        public abstract Buffer DecodeBuffer(byte[] Buff);

        protected int GetSequentialNumber(byte[] Buff) { return Buff[_sequentialNumberOffset]; }
    }
}
