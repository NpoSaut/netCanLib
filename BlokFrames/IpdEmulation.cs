using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlokFrames
{
    [FrameDescriptor(0xcfe8)]
    public class IpdEmulation : BlokFrame
    {
        /// <summary>
        /// Состояние ДПС 1
        /// </summary>
        public SensorState Sensor1State { get; set; }
        /// <summary>
        /// Состояние ДПС 2
        /// </summary>
        public SensorState Sensor2State { get; set; }

        protected override byte[] Encode()
        {
            using (var ms = new MemoryStream())
            {
                ms.Seek(0, SeekOrigin.Begin);
                Sensor1State.WriteTo(ms);
                ms.Seek(4, SeekOrigin.Begin);
                Sensor2State.WriteTo(ms);

                return ms.ToArray();
            }
        }
        protected override void Decode(byte[] Data) { throw new NotImplementedException(); }

        /// <summary>
        /// Направление вращения
        /// </summary>
        public enum RorationDirection : byte
        {
            /// <summary>
            /// По часовой стрелке
            /// </summary>
            Clockwise = 0,
            /// <summary>
            /// Против часовой стрелки
            /// </summary>
            Counterclockwise = 1
        }

        /// <summary>
        /// Состояние канала измерения
        /// </summary>
        public enum ChannelCondition : byte
        {
            /// <summary>
            /// Исправен
            /// </summary>
            Good = 0,
            /// <summary>
            /// Не исправен
            /// </summary>
            Bad = 1
        }

        /// <summary>
        /// Состояние датчика измерения скорости
        /// </summary>
        public class SensorState
        {
            /// <summary>
            /// Имитируемая частота импульсов
            /// </summary>
            public int Frequncy { get; set; }
            /// <summary>
            /// Направление вращения
            /// </summary>
            public RorationDirection Direction { get; set; }
            /// <summary>
            /// Достоверность измерения канала 1
            /// </summary>
            public ChannelCondition Channel1Condition { get; set; }
            /// <summary>
            /// Достоверность измерения канала 2
            /// </summary>
            public ChannelCondition Channel2Condition { get; set; }

            internal void WriteTo(Stream str)
            {
                var w = new BinaryWriter(str);
                w.Write((UInt16)(Frequncy/10));
                w.Write((Byte)(
                        (byte)Direction << 0 |
                        (byte)Channel1Condition << 1 |
                        (byte)Channel2Condition << 2
                        ));
            }
        }
    }
}
