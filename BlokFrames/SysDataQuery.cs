using System;

namespace BlokFrames
{
    /// <summary>
    ///     Сообщение используется для запроса значения одного из параметров, хранимых в массиве постоянных характеристик.
    ///     Обрабатывается модулем МПХ.
    /// </summary>
    [FrameDescriptor(0x0e01)]
    public class SysDataQuery : BlokFrame
    {
        public SysDataQuery() { }
        public SysDataQuery(Int32 Index) { this.Index = Index; }

        /// <summary>Номер запрашиваемого свойства</summary>
        public Int32 Index { get; set; }

        protected override byte[] Encode() { return new[] { (byte)Index }; }

        protected override void Decode(byte[] Data) { Index = Data[0]; }

        public override string ToString() { return string.Format("RQ {0}", Index); }
    }
}
