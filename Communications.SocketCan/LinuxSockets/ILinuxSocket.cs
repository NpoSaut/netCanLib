using System;
using Communications.Can;

namespace Communications.SocketCan.LinuxSockets
{
    /// <summary>Linux Can �����</summary>
    /// <remarks>��������� �������� ������, ��������� � �������� ��� ������, � ��� �� ������ � ������ �� ������ ������ ������</remarks>
    public interface ILinuxSocket : IDisposable
    {
        /// <summary>������ ������ �������� ���������</summary>
        int RxBufferSize { get; }

        /// <summary>������ ������ ��������� ���������</summary>
        int TxBufferSize { get; }

        /// <summary>������� ����� �������� ���������</summary>
        void FlushInBuffer();

        /// <summary>������ ��������� � ������� �� �������� � SocketCan</summary>
        /// <param name="Frame">��������� ��� ��������</param>
        /// <returns>���������� ���������, ����������� � �����</returns>
        int Send(CanFrame Frame);

        /// <summary>��������� ������ �� ������</summary>
        /// <param name="Timeout">������� �������� ������</param>
        /// <returns>���������, ����������� � ������ ������ �� ������ ������ ���������, ���� ��������� �� �����, ��������� �
        ///     <paramref name="Timeout" /> ����� � ������.</returns>
        CanFrame Receive(TimeSpan Timeout);
    }
}
