using System;
using System.Runtime.Serialization;

namespace SocketCanWorking.Exceptions
{
    /// <Summary>Ошибка при отчистке буфера Linux SocketCan</Summary>
    [Serializable]
    public class SocketCanFlushException : SocketCanException
    {
        public SocketCanFlushException(int ErrorCode) : base(String.Format("Ошибка #{0} при отчистке буфера Linux SocketCan", ErrorCode))
        {
            this.ErrorCode = ErrorCode;
        }

        protected SocketCanFlushException(
            SerializationInfo info,
            StreamingContext context, int ErrorCode) : base(info, context)
        {
            this.ErrorCode = ErrorCode;
        }

        public int ErrorCode { get; private set; }
    }
}
