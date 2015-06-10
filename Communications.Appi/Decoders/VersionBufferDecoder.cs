using System;
using Communications.Appi.Buffers;
using Buffer = Communications.Appi.Buffers.Buffer;

namespace Communications.Appi.Decoders
{
    public class VersionBufferDecoder : BufferDecoderBase
    {
        private readonly int _appiVersionOffset;

        public VersionBufferDecoder(int SequentialNumberOffset, int AppiVersionOffset) : base(SequentialNumberOffset)
        {
            _appiVersionOffset = AppiVersionOffset;
        }

        public override Buffer DecodeBuffer(byte[] Buff)
        {
            return new VersionReadAppiBuffer(GetSequentialNumber(Buff), new Version(Buff[_appiVersionOffset], 0));
        }
    }
}
