using System;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Communications.Protocols.IsoTP.Frames;

namespace IsoTpTest.FrameTests
{
    [TestClass]
    public class SingleFrameTest
    {
        private Random _random;
        byte[] Data { get; set; }

        [TestInitialize]
        public void Init()
        {
            _random = new Random();
            Data = new byte[_random.Next(7)];
            _random.NextBytes(Data);
        }

        [TestMethod]
        public void Single_Create()
        {
            var frame = new SingleFrame(Data);

            Assert.AreEqual(BitConverter.ToString(frame.Data), BitConverter.ToString(Data), "Значение свойства Data не соответствует переданным данным");
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Single_Create_ArgumentNullException()
        {
            Data = null;

            var frame = new SingleFrame(Data);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void Single_Create_ArgumentOutOfRangeException()
        {
            _random = new Random(8);
            Data = new byte[_random.Next(1000)];
            _random.NextBytes(Data);

            var frame = new SingleFrame(Data);
        }

        [TestMethod]
        public void Single_GetCanFrame()
        {
            const int descriptor = 0xfc08;
            var frame = new SingleFrame(Data);

            var canFrame = frame.GetCanFrame(descriptor);

            var result = new byte[8];
            result[0] = (byte)(((byte)frame.FrameType & 0x0f) << 4 | Data.Length & 0x0f);
            Buffer.BlockCopy(Data, 0, result, 1, Data.Length);

            Assert.AreEqual(canFrame.Descriptor, descriptor, "Значение свойства Descriptor не соответствует переданным данным");
            Assert.AreEqual(BitConverter.ToString(canFrame.Data), BitConverter.ToString(result), "Значение свойства Data не соответствует переданным данным");
        }

        [TestMethod]
        public void Single_FillWithBytes()
        {
            var buff = new byte[8];
            buff[0] = (byte)((0x0 & 0x0f) << 4 | Data.Length & 0x0f);
            Buffer.BlockCopy(Data, 0, buff, 1, Data.Length);

            var frame = IsoTpFrame.ParsePacket<SingleFrame>(buff);

            Assert.AreEqual(Data.Length, frame.Data.Length, "Рзмер массива Data не соответствует перданным данным");
            Assert.AreEqual(BitConverter.ToString(frame.Data), BitConverter.ToString(Data), "Значение свойства Data не соответствует переданным данным");
        }
    }
}
