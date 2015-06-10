using System;
using System.Runtime.Serialization;

namespace Communications.Appi.Exceptions
{
    /// <Summary>
    /// �������� ���� ��������
    /// </Summary>
    [Serializable]
    public class TransferAbortedException : AppiException
    {
        public TransferAbortedException() : base("�������� ���� ��������") { }
        public TransferAbortedException(Exception inner) : base("�������� ���� ��������", inner) { }
        public TransferAbortedException(string message) : base(message) { }
        public TransferAbortedException(string message, Exception inner) : base(message, inner) { }

        protected TransferAbortedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
}