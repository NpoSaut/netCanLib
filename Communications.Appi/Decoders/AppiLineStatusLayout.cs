namespace Communications.Appi.Decoders
{
    /// <summary>������������ �������� � Can-����� � ������ ������ ����</summary>
    public class AppiLineStatusLayout
    {
        /// <summary>�������������� ����� ��������� ������ <see cref="T:AppiLineStatusLayout" />.</summary>
        /// <param name="FramesBodyOffset">��������� ���� ���������</param>
        /// <param name="FramesCountOffset">��������� ���������� ���������</param>
        /// <param name="BaudRateOffset">��������� ������ � ��������� �������� �����</param>
        /// <param name="SendQueueOffset">��������� ������� ������� �� �������� �� ���� �����</param>
        /// <param name="SendErrorIndex">��������� ������� ������ ��������</param>
        /// <param name="ReceiveErrorIndex">��������� ������� ������ �����</param>
        public AppiLineStatusLayout(int FramesBodyOffset, int FramesCountOffset, int BaudRateOffset, int SendQueueOffset, int SendErrorIndex,
                                    int ReceiveErrorIndex)
        {
            this.FramesBodyOffset = FramesBodyOffset;
            this.FramesCountOffset = FramesCountOffset;
            this.BaudRateOffset = BaudRateOffset;
            this.SendQueueOffset = SendQueueOffset;
            this.SendErrorIndex = SendErrorIndex;
            this.ReceiveErrorIndex = ReceiveErrorIndex;
        }

        /// <summary>��������� ���� ���������</summary>
        public int FramesBodyOffset { get; private set; }

        /// <summary>��������� ���������� ���������</summary>
        public int FramesCountOffset { get; private set; }

        /// <summary>��������� ������ � ��������� �������� �����</summary>
        public int BaudRateOffset { get; private set; }

        /// <summary>��������� ������� ������� �� �������� �� ���� �����</summary>
        public int SendQueueOffset { get; private set; }

        /// <summary>��������� ������� ������ ��������</summary>
        public int SendErrorIndex { get; private set; }

        /// <summary>��������� ������� ������ �����</summary>
        public int ReceiveErrorIndex { get; private set; }
    }
}
