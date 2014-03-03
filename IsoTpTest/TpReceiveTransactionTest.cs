using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Communications.Can;
using Communications.Can.FrameEncoders;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.Frames.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IsoTpTest
{
    [TestClass]
    public class TpReceiveTransactionTest
    {
        private Random _random;
        [TestMethod]
        public void Rceive_FirstFrame()
        {
            _random = new Random();
            int[] desc = {0x2008, 0x3008};
            var messData = new byte[_random.Next(4096)];
            var dataCount = 0;
            _random.NextBytes(messData);
            var flowCount = 0;
            var blockSize = _random.Next(100);
            var frames = GetFrames(messData);
            var ffPacketSize = 0;
            var ffData = new byte[6];


            using (ShimsContext.Create())
            {
                #region Shims&Stubs
                Communications.Can.Fakes.ShimCanFrame.NewWithDescriptorInt32ByteArrayInt32 = (i, bytes, arg3) =>
                {
                    var descriptedLength = i%0x20;
                    var data = new Byte[descriptedLength];
                    Array.Copy(bytes, arg3, data, 0, Math.Min(descriptedLength, bytes.Length));
                    var id = i/0x20;
                    return new Communications.Can.Fakes.ShimCanFrame
                    {
                        IdGet = () => id,
                        DataGet = () => data
                    };
                };

                ShimFirstFrame.Constructor = frame => { };

                var f = new FirstFrame();
                var ff = new ShimFirstFrame(f)
                {
                    DataGet = () => ffData,
                    PacketSizeGet = () => ffPacketSize
                };
                ShimFirstFrame.ImplicitOpCanFrameFirstFrame = frame =>
                {
                    ffPacketSize = frame.Data[1] | (frame.Data[0] & 0x0f) << 8;

                    ffData = new Byte[6];
                    Buffer.BlockCopy(frame.Data, 2, ffData, 0, 6);

                    return f;
                };
                
                var iCanFlow = new Communications.Can.Fakes.StubICanFlow
                {
                    DescriptorsGet = () => new ReadOnlyCollection<int>(desc.Distinct().ToList()),
                    ReadTimeSpanBoolean = (timeSpan, boolean) => frames,
                    SendCanFrameBoolean = (frame, boolean) => { }
                };
                var tr = new TpReceiveTransaction(iCanFlow, 0x2008, 0x3008);

                var shimTr = new Communications.Protocols.IsoTP.Fakes.ShimTpReceiveTransaction(tr)
                {
                    PointerGet = () => dataCount,
                    BlockSizeGet = () => (byte) blockSize,
                    SendFlowControl = () => { flowCount++; },
                    ReadBlockIEnumerableOfCanFrame = enumerable =>
                    {
                        dataCount += blockSize;
                        //enumerable = enumerable.Skip(blockSize);
                    }
                };
            #endregion

                var receive = tr.Receive();

                Assert.AreEqual(messData.Length, dataCount, "Приняты не все данные.");
                Assert.AreEqual(messData.Length / blockSize, flowCount, "Отправлено слишком мало FlowContol'ов");
                Assert.IsTrue(ffPacketSize != 0);
                CollectionAssert.AllItemsAreNotNull(ffData);
            }
        }

        private IEnumerable<CanFrame> GetSingleFrame()
        {
            _random = new Random();
            var buff = new byte[8];
            _random.NextBytes(buff);
            buff[0] = (byte)((0x0 & 0x0f) << 4 | (buff.Length - 1) & 0x0f);
            return new List<CanFrame>{CanFrame.NewWithDescriptor(0x2008, buff)};
        }
        
        private CanFrame GetFirstFrame(byte[] data,int packetSize)
        {
            var firstFrame = new byte[8];
            firstFrame[0] = (byte)((0x01 & 0x0f) << 4 | (packetSize & 0xf00) >> 8);
            firstFrame[1] = (byte)(packetSize & 0x0ff);
            Buffer.BlockCopy(data, 0, firstFrame, 2, data.Length);
            return CanFrame.NewWithDescriptor(0x2008, firstFrame);
        }

        private IEnumerable<CanFrame> GetFrames(ICollection<byte> data)
        {
            var frameIndex = 1;
            var frames = new List<CanFrame> {GetFirstFrame(data.Take(6).ToArray(), data.Count)};
            for (var i = 0; i < data.Count; i+=7)
            {
                var buff = new byte[8];
                buff[0] = (byte)((0x02 & 0x0f) << 4 | frameIndex++ & 0x0f);
                Buffer.BlockCopy(data.ToArray(), i, buff, 1, i + 7 > data.Count ? data.Count - i : 7);
                frames.Add(CanFrame.NewWithDescriptor(0x2008, buff));
            }
            return frames;
        }
    }
}
