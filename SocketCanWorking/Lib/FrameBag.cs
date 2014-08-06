using System;
using System.Runtime.InteropServices;

namespace SocketCanWorking.Lib
{
    /// <summary>Флаги в структуре передачи CAN-кадра из SocketCan библиотеки.</summary>
    [Flags]
    public enum FrameBagFlags : byte
    {
        /// <summary>Показывает, что пакет является Loopback-пакетом.</summary>
        /// <remarks>Loopback-пакеты в SocketCan используются для подтверждения отправки сообщения.</remarks>
        Loopback = 0x01
    }

    /// <summary>Структура передачи CAN-кадра из SocketCan библиотеки.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FrameBag
    {
        /// <summary>Сообщение.</summary>
        public readonly SocketCanFdFrame Frame;

        /// <summary>Время принятия сообщения.</summary>
        public readonly TimeVal ReceiveTime;

        /// <summary>Флаги принятого сообщения.</summary>
        public readonly FrameBagFlags Flags;

        public override string ToString() { return String.Format("ReceiveTime: {1}, Flags: {2}, Frame: ({0})", Frame, ReceiveTime, Flags); }
    }
}