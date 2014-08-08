using System;
using System.Collections.Generic;
using Communications.Can;

namespace SocketCanWorking.LinuxSockets
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
        /// <param name="Frames">��������� ��� ��������</param>
        /// <returns>���������� ���������, ����������� � �����</returns>
        int Send(IList<CanFrame> Frames);

        /// <summary>��������� ������ �� ������</summary>
        /// <param name="Timeout">������� �������� ������</param>
        /// <returns>���������, ����������� � ������ ������ �� ������ ������ ���������, ���� ��������� �� �����, ��������� �
        ///     <paramref name="Timeout" /> ����� � ������.</returns>
        IList<CanFrame> Receive(TimeSpan Timeout);
    }
}
