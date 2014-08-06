using System.Collections.Generic;
using Communications.Can;

namespace SocketCanWorking
{
    /// <summary>���������� �� �������� ��������� � �����</summary>
    public interface ISocketCanWriter
    {
        /// <summary>��������� ����������� �������� ��������� � �����</summary>
        void Send(IList<CanFrame> Frames);
    }
}
