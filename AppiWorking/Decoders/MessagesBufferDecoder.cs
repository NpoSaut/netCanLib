using System.Collections.Generic;
using System.Linq;
using Communications.Appi.Buffers;

namespace Communications.Appi.Decoders
{
    public abstract class MessagesBufferDecoder<TLineKey> : BufferDecoderBase
    {
        protected MessagesBufferDecoder(int SequentialNumberOffset) : base(SequentialNumberOffset) { }

        protected IDictionary<TLineKey, AppiLineStatus> GetLineStatuses(byte[] Buff, IDictionary<TLineKey, AppiLineStatusDecoder> LineStatusDecoders)
        {
            return LineStatusDecoders.ToDictionary(x => x.Key, x => x.Value.DecodeLineStatus(Buff));
        }
    }
}
