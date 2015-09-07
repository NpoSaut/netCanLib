using System;
using System.Collections.Generic;
using Communications.Can;
using SocketCanWorking.Exceptions;

namespace SocketCanWorking.Lib
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
        int Write(int SocketNumber, IList<CanFrame> Frames);

        /// <summary>�������� ��������� ������ �� ������.</summary>
        /// <param name="SocketNumber">����� ������ ��� ������.</param>
        /// <param name="Timeout">������� �������� ��������� ��������� � ������, ���� �� �������� ������ �� ��������� ���������.</param>
        /// <returns>������ �������, ����������� �� ���������� ������.</returns>
        IList<CanFrame> Read(int SocketNumber, TimeSpan Timeout);

        /// <summary>��������� �������� ������ �������� ��������� ��� ���������� ������</summary>
        /// <param name="SocketNumber">����� ������, � ������� ��������� ��������� ����� �������� ���������</param>
        void FlushInBuffer(int SocketNumber);
    }
}
