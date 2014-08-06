using System;
using System.Runtime.InteropServices;

namespace SocketCanWorking.Lib
{
    /// <summary>����� � ��������� �������� CAN-����� �� SocketCan ����������.</summary>
    [Flags]
    public enum FrameBagFlags : byte
    {
        /// <summary>����������, ��� ����� �������� Loopback-�������.</summary>
        /// <remarks>Loopback-������ � SocketCan ������������ ��� ������������� �������� ���������.</remarks>
        Loopback = 0x01
    }

    /// <summary>��������� �������� CAN-����� �� SocketCan ����������.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FrameBag
    {
        /// <summary>���������.</summary>
        public readonly SocketCanFdFrame Frame;

        /// <summary>����� �������� ���������.</summary>
        public readonly TimeVal ReceiveTime;

        /// <summary>����� ��������� ���������.</summary>
        public readonly FrameBagFlags Flags;

        public override string ToString() { return String.Format("ReceiveTime: {1}, Flags: {2}, Frame: ({0})", Frame, ReceiveTime, Flags); }
    }
}