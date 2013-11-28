using System;
using System.IO;

namespace Communications.Can.FrameEncoders
{
    public class FrameSbsEncoder : FrameStreamEncoder
    {
        public override CanFrame DecodeNext(Stream DataStream)
        {
            var headerBuffer = new Byte[8 + 2];
            if (DataStream.Read(headerBuffer, 0, headerBuffer.Length) < headerBuffer.Length)
                return null;
            var dt = DateTime.FromBinary(BitConverter.ToInt64(headerBuffer, 0));
            UInt16 descriptor = BitConverter.ToUInt16(headerBuffer, 8);

            var frame = CanFrame.NewWithDescriptor(descriptor);
            DataStream.Read(frame.Data, 0, frame.Data.Length);
            frame.Time = dt;
            return frame;
        }

        public override void Write(CanFrame Frame, Stream DataStream)
        {
            int length = Frame.Data.Length + 2 + 8;
            var buff = new Byte[length];
            Buffer.BlockCopy(BitConverter.GetBytes(Frame.Time.ToBinary()), 0, buff, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)Frame.Descriptor), 0, buff, 8, 2);
            Buffer.BlockCopy(Frame.Data, 0, buff, 8 + 2, Frame.Data.Length);

            DataStream.Write(buff, 0, buff.Length);
        }
    }
}
