using System;
using System.Collections.Generic;
using Communications.Can;

namespace SocketCanWorking.LinuxSockets
{
    /// <summary>Linux Can �����</summary>
    /// <remarks>��������� �������� ������, ��������� � �������� ��� ������, � ��� �� ������ � ������ �� ������ ������ ������</remarks>
    public interface ILinuxSocket : IDisposable
    {
        /// <summary>������� ����� �������� ���������</summary>
        void FlushInBuffer();

        /// <summary>������ ��������� � ������� �� �������� � SocketCan</summary>
        /// <param name="Frames">��������� ��� ��������</param>
        void Send(IList<CanFrame> Frames);

        /// <summary>��������� ������ �� ������</summary>
        /// <param name="Timeout">������� �������� ������</param>
        /// <returns>���������, ����������� � ������ ������ �� ������ ������ ���������, ���� ��������� �� �����, ��������� �
        ///     <paramref name="Timeout" /> ����� � ������.</returns>
        IList<CanFrame> Receive(TimeSpan Timeout);
    }
}
