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
            public int Frequency { get; set; }
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
                w.Write((UInt16)(Frequency/10));
                w.Write((Byte)(
                        (byte)Direction << 0 |
                        (byte)Channel1Condition << 1 |
                        (byte)Channel2Condition << 2
                        ));
            }

            public enum DpsSensorPlacement { Left, Right }

            /// <summary>Получает <see cref="SensorState" /> исходя из значений скорости, числа зубьев и диаметра бандажа</summary>
            /// <remarks>Эмпирически выяснилось, что значение скорости необходимо будет умножить на 160 :-/. Формула взята у Юры из исходника программы АППИ.</remarks>
            /// <param name="Speed">Значение скорости (с учётом знака: + вперёд, - назад)</param>
            /// <param name="CogsCount">Количество импульсов (зубьев ДПС) на оборот колеса</param>
            /// <param name="BondageDiameter">Диаметре бандажа колеса</param>
            /// <param name="SensorPlacement">Размещение датчика скорости</param>
            /// <param name="Channel1Condition">Достоверность измерения канала 1</param>
            /// <param name="Channel2Condition">Достоверность измерения канала 2</param>
            /// <returns>Элемент <see cref="SensorState" />, соответствующий заданному состоянию датчика</returns>
            public static SensorState Get(Double Speed, int CogsCount, Double BondageDiameter, DpsSensorPlacement SensorPlacement, ChannelCondition Channel1Condition = ChannelCondition.Good, ChannelCondition Channel2Condition = ChannelCondition.Good)
            {
                return new SensorState
                       {
                           Channel1Condition = Channel1Condition,
                           Channel2Condition = Channel2Condition,
                           Frequency =
                               (int) Math.Round((Math.Abs(Speed) * 88.41941282883074209382431298473 * CogsCount) / BondageDiameter), // Почему-то эмпирически выяснено, что нужно умножать скорость на 160 :-/
                           Direction = Speed > 0
                                           ? (SensorPlacement == DpsSensorPlacement.Left
                                                  ? RorationDirection.Clockwise
                                                  : RorationDirection.Counterclockwise)
                                           : (SensorPlacement == DpsSensorPlacement.Right
                                                  ? RorationDirection.Clockwise
                                                  : RorationDirection.Counterclockwise)
                       };
            }
        }
    }
}
