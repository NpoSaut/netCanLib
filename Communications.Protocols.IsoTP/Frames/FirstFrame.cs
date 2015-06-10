using System;
using Communications.Can;

namespace Communications.Protocols.IsoTP.Frames
{
    /// <summary>Первый кадр транзакции</summary>
    /// <remarks>Этим кадром инициируется транзакция</remarks>
    public class FirstFrame : IsoTpFrame
    {
        /// <summary>Вместимость пакета</summary>
        public const int DataCapacity = 6;

        public FirstFrame(Byte[] Data, int PacketSize)
        {
            if (Data.Length != DataCapacity)
                throw new ArgumentOutOfRangeException("Data",
                                                      string.Format("Первый фрейм должен содержать ровно {0} байт данных",
                                                                    DataCapacity));

            this.Data = Data;
            this.PacketSize = PacketSize;
        }

        public FirstFrame() { }

        /// <summary>Размер полного пакета</summary>
        public int PacketSize { get; private set; }

        /// <summary>Данные</summary>
        public Byte[] Data { get; private set; }

        public override IsoTpFrameType FrameType
        {
            get { return IsoTpFrameType.First; }
        }

        public override CanFrame GetCanFrame(int WithDescriptor)
        {
            var buff = new Byte[8];

            buff[0] = (byte)(((byte)FrameType & 0x0f) << 4 | (PacketSize & 0xf00) >> 8);
            buff[1] = (byte)(PacketSize & 0x0ff);

            Buffer.BlockCopy(Data, 0, buff, 2, DataCapacity);

            return CanFrame.NewWithDescriptor(WithDescriptor, buff);
        }

        protected override void FillWithBytes(byte[] buff)
        {
            PacketSize = buff[1] | (buff[0] & 0x0f) << 8;

            Data = new Byte[DataCapacity];
            Buffer.BlockCopy(buff, 2, Data, 0, DataCapacity);
        }

        public static implicit operator FirstFrame(CanFrame cFrame) { return ParsePacket<FirstFrame>(cFrame.Data); }

        public static int GetPayload(int SubframeLength) { return SubframeLength - 2; }

        public override string ToString()
        {
            return string.Format("FF: {0}", BitConverter.ToString(Data, 0, Data.Length));
        }
    }
}
