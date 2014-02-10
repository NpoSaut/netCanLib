﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Communications.Protocols.IsoTP.Frames;

namespace IsoTpUnitTest.FramesTest
{
    [TestClass]
    public class ConsecutiveFrameTest
    {
        private Random _rnd;
        [TestMethod]
        public void CreateConsecutiveFrameTest ()
        {
            _rnd = new Random();
            var index = _rnd.Next(9);
            var data = new byte[7];
            _rnd.NextBytes(data);
            var consecutiveFrame = new ConsecutiveFrame(data, index);

            Assert.AreEqual(consecutiveFrame.Index, index, "Значение свойства Index не соответствует переданному значению.");
            Assert.AreEqual(BitConverter.ToString(consecutiveFrame.Data), BitConverter.ToString(data), "Значение свойства Data не соответствует переданному значению.");
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void CreateFrameTest_OutOfRangeException()
        {
            _rnd = new Random();
            var data = new byte[_rnd.Next(8, 100)];
            _rnd.NextBytes(data);

            var consecutiveFrame = new ConsecutiveFrame(data, 1);
        }

        [TestMethod]
        
        public void GetCanFrameTest()
        {
            _rnd = new Random();
            const int descriptor = 0xfc08;
            var index = _rnd.Next(9);
            var data = new byte[7];
            _rnd.NextBytes(data);
            
            var consecutiveFrame = new ConsecutiveFrame(data, index);

            var consCanFrame = consecutiveFrame.GetCanFrame(descriptor);

            var result = string.Format("{0:X2}-{1}",(byte)((0x2 & 0x0f) << 4 | index & 0x0f), BitConverter.ToString(consecutiveFrame.Data));

            Assert.AreEqual(descriptor, consCanFrame.Descriptor, "Ошибка при вычислении дескриптора");
            Assert.AreEqual(result, BitConverter.ToString(consCanFrame.Data), "Ошибка при записи данных");
        }

        [TestMethod]
        public void FillWithBytesTest()
        {
            _rnd = new Random();
            var index = _rnd.Next(9);
            var data = new byte[8];
            _rnd.NextBytes(data);
            data[0] = (byte) ((0x2 & 0x0f) << 4 | index & 0x0f);


            var consecutiveFrame = IsoTpFrame.ParsePacket<ConsecutiveFrame>(data);
            
            Assert.AreEqual(BitConverter.ToString(consecutiveFrame.Data), BitConverter.ToString(data, 1), "Ошибка при заполнении данными");
            Assert.AreEqual(index, consecutiveFrame.Index, "Ошибка при вычислени индекса");
        }
    }
}
