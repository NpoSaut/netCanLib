using System;
using System.Collections.Generic;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;
using NUnit.Framework;

namespace IsoTpTest.FrameTests
{
    [TestFixture]
    public class FlowControlFrameTests
    {
        private Random _random;
        private readonly Dictionary<int, FlowControlFlag> _flagValue = new Dictionary<int, FlowControlFlag>();
        private FlowControlFlag Flag { get; set; }
        private byte SeparationTimeCode { get; set; }
        private TimeSpan SeparationTime { get; set; }
        private byte BlockSize { get; set; }

        [SetUp]
        public void Init()
        {
            _random = new Random();
            _flagValue[0] = FlowControlFlag.ClearToSend;
            _flagValue[1] = FlowControlFlag.Wait;
            _flagValue[2] = FlowControlFlag.Abort;
            Flag = _flagValue[_random.Next(2)];
            SeparationTimeCode = (byte)_random.Next(150);
            SeparationTime = TimeSpan.FromMilliseconds(_random.Next(300));
            BlockSize = (byte)_random.Next(4095);
        }

        private Byte SeparationCodeFromTime(TimeSpan time)
        {
            if (time == TimeSpan.Zero) return 0;
            return time.TotalMilliseconds < 1
                       ? (byte)(time.TotalMilliseconds * 10 + 0xf0)
                       : (byte)Math.Min(time.TotalMilliseconds, 0x7f);
        }

        private TimeSpan SeparationTimeFromCode(Byte code)
        {
            return code < 0x7f
                       ? TimeSpan.FromMilliseconds(code)
                       : TimeSpan.FromMilliseconds(1);
        }

        [Test]
        public void FlowControl_CreateFrame_TimeCode()
        {
            var frame = new FlowControlFrame(Flag, BlockSize, SeparationTimeCode);

            Assert.AreEqual(frame.SeparationTime, SeparationTimeFromCode(SeparationTimeCode),
                            "Значение свйоства SeparationTime не совпадает с переданными данными");
            Assert.AreEqual(frame.BlockSize, BlockSize, "Значение свйоства BlockSize не совпадает с переданными данными");
            Assert.AreEqual((byte)frame.Flag, (byte)Flag, "Значение свйоства Flag не совпадает с переданными данными");
        }

        [Test]
        public void FlowControl_Create_TimeSpan()
        {
            var frame = new FlowControlFrame(Flag, BlockSize, SeparationTime);

            Assert.AreEqual(frame.SeparationTime, SeparationTime, "Значение свйоства SeparationTime не совпадает с переданными данными");
            Assert.AreEqual(frame.BlockSize, BlockSize, "Значение свйоства BlockSize не совпадает с переданными данными");
            Assert.AreEqual((byte)frame.Flag, (byte)Flag, "Значение свйоства Flag не совпадает с переданными данными");
        }

        [Test]
        public void FlowControl_FillWithBytes()
        {
            var frame = new FlowControlFrame();

            byte[] buff =
            {
                (byte)(((byte)frame.FrameType & 0x0f) << 4 | (byte)Flag & 0x0f), BlockSize,
                SeparationTimeCode
            };

            frame = IsoTpFrame.ParsePacket<FlowControlFrame>(buff);

            Assert.AreEqual((byte)frame.Flag, (byte)Flag, "Значение свйоства Flag не совпадает с переданными данными");
            Assert.AreEqual(frame.SeparationTime, SeparationTimeFromCode(SeparationTimeCode),
                            "Значение свйоства SeparationTime не совпадает с переданными данными");
            Assert.AreEqual(frame.BlockSize, BlockSize, "Значение свйоства BlockSize не совпадает с переданными данными");
        }

        [Test]
        public void FlowControl_GetCanFrame()
        {
            const int descriptor = 0xfc08;
            var frame = new FlowControlFrame(Flag, BlockSize, SeparationTime);

            CanFrame canFrame = frame.GetCanFrame(descriptor);

            byte[] buff =
            {
                (byte)(((byte)frame.FrameType & 0x0f) << 4 | (byte)Flag & 0x0f), BlockSize,
                SeparationCodeFromTime(SeparationTime), 0, 0, 0, 0, 0
            };

            Assert.AreEqual(canFrame.Descriptor, descriptor);
            Assert.AreEqual(BitConverter.ToString(canFrame.Data), BitConverter.ToString(buff));
        }
    }
}
