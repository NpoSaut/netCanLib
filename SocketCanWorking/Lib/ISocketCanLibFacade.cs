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
        /// <param name="InterfaceName">��� ������ � ���� c-������.</param>
        /// <exception cref="SocketCanOpenException">������ ��� ������� ������� �����.</exception>
        int Open(String InterfaceName);

        /// <summary>��������� �����.</summary>
        /// <param name="Number">����� ������.</param>
        void Close(int Number);

        /// <summary>���������� CAN-�����.</summary>
        /// <param name="SocketNumber">����� ������ ��� ��������.</param>
        /// <param name="Frame">����� ��� ��������.</param>
        void Write(int SocketNumber, IList<CanFrame> Frame);

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
