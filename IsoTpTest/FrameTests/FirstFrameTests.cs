﻿using System;
using Communications.Protocols.IsoTP.Frames;
using NUnit.Framework;

namespace IsoTpTest.FrameTests
{
    [TestFixture]
    public class FirstFrameTests
    {
        private Random _random;

        [Test]
        public void First_Create()
        {
            _random = new Random();
            var packetSize = _random.Next(4096);
            var data = new byte[6];
            _random.NextBytes(data);

            var firstFrame = new FirstFrame(data, packetSize);

            Assert.AreEqual(packetSize, firstFrame.PacketSize, "Значение свойства PacketSize не соответствует переданному числу.");
            Assert.AreEqual(BitConverter.ToString(data), BitConverter.ToString(firstFrame.Data), "Значение свойства Data не соответствует переданному массиву");
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void First_CreateFrame_OutOfRangeException()
        {
            _random = new Random();
            var data = new byte[6];
            while (data.Length == 6)
            {
                data = new byte[_random.Next(100)];
            }
            _random.NextBytes(data);

            var firstFrame = new FirstFrame(data, 100);
        }

        [Test]
        public void First_GetCanFrame()
        {
            _random = new Random();
            const int descriptor = 0xfc08;
            var packetSize = _random.Next(4096);
            var data = new byte[6];
            _random.NextBytes(data);

            var firstFrame = new FirstFrame(data, packetSize);

            var firstCanFrame = firstFrame.GetCanFrame(descriptor);

            var result = string.Format("{0:X2}-{1:X2}-{2}", (byte)(((byte)firstFrame.FrameType & 0x0f) << 4 | (packetSize & 0xf00) >> 8), (byte)(packetSize & 0x0ff), BitConverter.ToString(firstFrame.Data));

            Assert.AreEqual(firstCanFrame.Descriptor, descriptor, "Ошибка при вычислении дескриптора");
            Assert.AreEqual(BitConverter.ToString(firstCanFrame.Data), result, "Ошибка при записи данных");
        }

        [Test]
        public void First_FillWithBytes()
        {
            _random = new Random();
            var packetSize = _random.Next(4096);
            var data = new byte[8];
            _random.NextBytes(data);
            var firstFrame = new FirstFrame();
            data[0] = (byte)(((byte)firstFrame.FrameType & 0x0f) << 4 | (packetSize & 0xf00) >> 8);
            data[1] = (byte) (packetSize & 0x0ff);


            firstFrame = IsoTpFrame.ParsePacket<FirstFrame>(data);

            Assert.AreEqual(BitConverter.ToString(data, 2), BitConverter.ToString(firstFrame.Data), "Ошибка при заполнении данными");
            Assert.AreEqual(firstFrame.PacketSize, packetSize, "Ошибка при вычислени размера пакета");
        }
    }
}
