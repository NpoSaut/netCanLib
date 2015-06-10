using System.Collections.Generic;
using Communications.Appi.Devices;

namespace Communications.Appi.Buffers
{
    /// <summary>Буфер АППИ БЛОК со списком принятых сообщений</summary>
    public class AppiBlockMessagesBuffer : MessagesBuffer<AppiLine>
    {
        public AppiBlockMessagesBuffer(int SequentialNumber, IDictionary<AppiLine, AppiLineStatus> LineStatuses) : base(SequentialNumber, LineStatuses) { }
    }
}
