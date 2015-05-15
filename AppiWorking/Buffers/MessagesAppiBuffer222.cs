using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Can;

namespace Communications.Appi.Buffers
{
    [AppiBufferIdentifer(0x02)]
    class MessagesAppiBuffer222 : Buffer 
    {
        public Dictionary<AppiLine, IList<CanFrame>> CanMessages { get; set; }
        private static readonly Dictionary<AppiLine, int> CanMessagesOffsets = 
            new Dictionary<AppiLine, int>
            {
                { AppiLine.Can1, 24 },
                { AppiLine.Can2, 524 }
            };

        public Byte[] SerialBuffer { get; set; }

        public Dictionary<AppiLine, int> OutMessagesCount { get; set; }
        public int SpeedA { get; set; }
        public int SpeedB { get; set; }

        public MessagesAppiBuffer222()
        {
            CanMessages = new Dictionary<AppiLine, IList<CanFrame>>
                          {
                              {AppiLine.Can1, new List<CanFrame>()},
                              {AppiLine.Can2, new List<CanFrame>()}
                          };
            OutMessagesCount = new Dictionary<AppiLine, int>
                               {
                                   {AppiLine.Can1, 0},
                                   {AppiLine.Can2, 0}
                               };
            SerialBuffer = new byte[0];
        }

        public override byte[] Encode() { throw new System.NotImplementedException(); }

        protected override void DecodeIt(byte[] buff)
        {
            int messagesInA = buff[6];
            int messagesInB = buff[2];

            CanMessages = new Dictionary<AppiLine, IList<CanFrame>>
                       {
                           { AppiLine.Can1, ParseBuffer(buff, CanMessagesOffsets[AppiLine.Can1], messagesInA).ToList() },
                           { AppiLine.Can2, ParseBuffer(buff, CanMessagesOffsets[AppiLine.Can2], messagesInB).ToList() }
                       };

            SpeedA = buff[7] | buff[8] << 8;
            SpeedB = buff[9] | buff[10] << 8;

            OutMessagesCount[AppiLine.Can1] = BitConverter.ToUInt16(buff, 17);
            OutMessagesCount[AppiLine.Can2] = BitConverter.ToUInt16(buff, 19);

            int bytesInSerial = buff[1024];
            SerialBuffer = new byte[bytesInSerial];
            System.Buffer.BlockCopy(buff, 1025, SerialBuffer, 0, SerialBuffer.Length);
        }

        /// <summary>
        /// Парсит буфер сообщений АППИ
        /// </summary>
        /// <param name="Buff">Буфер сообщений</param>
        /// <param name="Offset">Отступ от начала буфера</param>
        /// <param name="Count">Количество сообщений в буфере</param>
        private IEnumerable<CanFrame> ParseBuffer(Byte[] Buff, int Offset, int Count)
        {
            Byte[] buff = new Byte[10];
            for (int i = 0; i < Count; i++)
            {
                System.Buffer.BlockCopy(Buff, Offset + i * buff.Length, buff, 0, buff.Length);
                yield return AppiCanFrameConstructor.FromBufferBytes(buff);
            }
        }
    }
}