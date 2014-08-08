using System;
using System.Runtime.InteropServices;

namespace SocketCanWorking.Lib
{
    /// <summary>‘лаги в структуре передачи CAN-кадра из SocketCan библиотеки.</summary>
    [Flags]
    public enum FrameBagFlags : byte
    {
        /// <summary>ѕоказывает, что пакет €вл€етс€ Loopback-пакетом.</summary>
        /// <remarks>Loopback-пакеты в SocketCan используютс€ дл€ подтверждени€ отправки сообщени€.</remarks>
        Loopback = 0x01
    }

    /// <summary>—труктура передачи CAN-кадра из SocketCan библиотеки.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FrameBag
    {
        /// <summary>—ообщение.</summary>
        public readonly SocketCanFdFrame Frame;

        /// <summary>¬рем€ прин€ти€ сообщени€.</summary>
        public readonly TimeVal ReceiveTime;

        /// <summary>‘лаги прин€того сообщени€.</summary>
        public readonly FrameBagFlags Flags;

        /// <summary> оличество сообщений, потер€нных между последним прин€тым сообщением и этим сообщением</summary>
        public readonly UInt32 DroppedMessagesCount;

        public override string ToString() { return String.Format("ReceiveTime: {1}, Flags: {2}, Dropped: {3}, Frame: ({0})", Frame, ReceiveTime, Flags, DroppedMessagesCount); }
    }
}
