using System;
using System.Runtime.InteropServices;

namespace Communications.SocketCan.Lib
{
    /// <summary>������ ��������� TimeVal.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TimeVal
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public TimeVal(uint UnixTime, uint Microseconds)
        {
            this.UnixTime = UnixTime;
            this.Microseconds = Microseconds;
        }

        /// <summary>����� ����� ��������� � ��������� �� ������� (� ������� UNIX Time).</summary>
        public readonly UInt32 UnixTime;

        /// <summary>����� ����� ��������� � ������������� � ������ ������� �������.</summary>
        public readonly UInt32 Microseconds;

        public static implicit operator DateTime(TimeVal tv) { return Epoch.AddSeconds(tv.UnixTime).AddMilliseconds((Double)tv.Microseconds / 1000); }

        public override string ToString() { return String.Format("{0}.{1:000000}s", UnixTime, Microseconds); }
    }
}