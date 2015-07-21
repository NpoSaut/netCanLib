using System;

namespace Communications.Protocols.IsoTP
{
    public class IsoTpConnectionParameters
    {
        /// <summary>Размер блока при передаче длинных пакетов</summary>
        public int BlockSize { get; private set; }

        /// <summary>Время между передачей последовательных фреймов</summary>
        public TimeSpan SeparationTime { get; private set; }

        /// <summary>Время ожидания первого ответа (FlowControl-кадра) от собеседника</summary>
        public TimeSpan FirstResponseTimeout { get; private set; }

        /// <summary>Время ожидания последовательных сообщения (когда соединение уже устоялось)</summary>
        public TimeSpan ConsecutiveTimeout { get; private set; }

        public override string ToString() { return string.Format("BlockSize: {0}, SeparationTime: {1}", BlockSize, SeparationTime); }
    }
}
