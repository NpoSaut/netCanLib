using System;
using Communications.Can;
using Communications.SocketCan.Exceptions;

namespace Communications.SocketCan.Lib
{
    /// <summary>������ ��� ������������� ���������, ���������� ����������� ������ c-���� ������� � ������ � �����������</summary>
    public interface ISocketCanLibFacade
    {
        /// <summary>��������� �����.</summary>
        /// <param name="InterfaceName">��� ������</param>
        /// <param name="RxBuffSize">������ ������ �������� ���������</param>
        /// <param name="TxBuffSize">������ ������ ��������� ���������</param>
        /// <exception cref="SocketCanOpenException">������ ��� ������� ������� �����.</exception>
        int Open(string InterfaceName, int RxBuffSize, int TxBuffSize);

        /// <summary>��������� �����.</summary>
        /// <param name="Number">����� ������.</param>
        void Close(int Number);

        /// <summary>���������� CAN-�����.</summary>
        /// <param name="SocketNumber">����� ������ ��� ��������.</param>
        /// <param name="Frames">����� ��� ��������.</param>
        /// <returns>���������� ���������, ����������� � �����</returns>
        int Write(int SocketNumber, CanFrame Frames);

        /// <summary>�������� ��������� ������ �� ������.</summary>
        /// <param name="SocketNumber">����� ������ ��� ������.</param>
        /// <param name="Timeout">������� �������� ��������� ��������� � ������, ���� �� �������� ������ �� ��������� ���������.</param>
        /// <returns>������ �������, ����������� �� ���������� ������.</returns>
        CanFrame Read(int SocketNumber, TimeSpan Timeout);

        /// <summary>��������� �������� ������ �������� ��������� ��� ���������� ������</summary>
        /// <param name="SocketNumber">����� ������, � ������� ��������� ��������� ����� �������� ���������</param>
        void FlushInBuffer(int SocketNumber);
    }
}
