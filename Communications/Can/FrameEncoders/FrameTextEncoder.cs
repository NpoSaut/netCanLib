using System;
using System.Linq;
using System.Text;
using System.IO;

namespace Communications.Can.FrameEncoders
{
    public class FrameTextEncoder : FrameStreamEncoder
    {
        public String OutFormat { get; set; }
        public String ReadPattern { get; set; }

        public FrameTextEncoder()
        {
            OutFormat = "{2}\t{0:X4}\t{1}";
        }

        public override CanFrame DecodeNext(Stream DataStream)
        {
            throw new NotImplementedException();
        }

        public override void Write(CanFrame Frame, Stream DataStream)
        {
            string str = string.Format(OutFormat, Frame.Descriptor, string.Join(" ", Frame.Data.Select(b => b.ToString("X2"))), Frame.Time.ToString(System.Globalization.CultureInfo.InvariantCulture));
            var sBuff = Encoding.Default.GetBytes(str + Environment.NewLine);
            DataStream.Write(sBuff, 0, sBuff.Length);
        }
    }
}
