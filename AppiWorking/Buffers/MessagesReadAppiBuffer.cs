using System;
using System.Collections.Generic;
using System.Linq;
using Communications.Can;

namespace Communications.Appi.Buffers
{
    [AppiBufferIdentifer(0x02)]
    internal class MessagesReadAppiBuffer : AppiBufferBase
    {
        private static readonly Dictionary<AppiLine, int> CanMessagesOffsets =
            new Dictionary<AppiLine, int>
            {
                { AppiLine.Can1, 24 },
                { AppiLine.Can3, 524 },
                { AppiLine.CanTeh, 225 }
            };

        private IDictionary<AppiLine, int> _speeds;

        public MessagesReadAppiBuffer()
        {
            SerialBuffer = new byte[0];
        }

        public Dictionary<AppiLine, IList<CanFrame>> CanMessages { get; set; }

        public Byte[] SerialBuffer { get; set; }

        public Dictionary<AppiLine, int> OutMessagesCount { get; set; }

        public IDictionary<AppiLine, int> Speeds
        {
            get { return _speeds; }
        }

        public override byte[] Encode() { throw new NotImplementedException(); }

        protected override void DecodeIt(byte[] buff)
        {
            int messagesInCan1 = buff[6];
            int messagesInCan3 = buff[3];
            int messagesInCanTeh = buff[2];

            CanMessages = new Dictionary<AppiLine, IList<CanFrame>>
                          {
                              { AppiLine.Can1, ParseBuffer(buff, CanMessagesOffsets[AppiLine.Can1], messagesInCan1).ToList() },
                              { AppiLine.Can3, ParseBuffer(buff, CanMessagesOffsets[AppiLine.Can3], messagesInCan3).ToList() },
                              { AppiLine.CanTeh, ParseBuffer(buff, CanMessagesOffsets[AppiLine.CanTeh], messagesInCanTeh).ToList() }
                          };

            _speeds = new Dictionary<AppiLine, int>
                      {
                          { AppiLine.Can1, buff[7] | buff[8] << 8 },
                          { AppiLine.CanTeh, buff[9] | buff[10] << 8 },
                          { AppiLine.Can3, buff[15] | buff[16] << 8 },
                          { AppiLine.Can4, buff[17] | buff[18] << 8 }
                      };

            OutMessagesCount = new Dictionary<AppiLine, int>
                               {
                                   { AppiLine.Can1, buff[424] },
                                   { AppiLine.CanTeh, buff[425] },
                                   { AppiLine.Can3, buff[426] }
                               };

            int bytesInSerial = buff[1024];
            SerialBuffer = new byte[bytesInSerial];
            Buffer.BlockCopy(buff, 1025, SerialBuffer, 0, SerialBuffer.Length);
        }

        /// <summary>Парсит буфер сообщений АППИ</summary>
        /// <param name="Buff">Буфер сообщений</param>
        /// <param name="Offset">Отступ от начала буфера</param>
        /// <param name="Count">Количество сообщений в буфере</param>
        private IEnumerable<CanFrame> ParseBuffer(Byte[] Buff, int Offset, int Count)
        {
            var buff = new Byte[10];
            for (int i = 0; i < Count; i++)
            {
                Buffer.BlockCopy(Buff, Offset + i * buff.Length, buff, 0, buff.Length);
                yield return AppiCanFrameConstructor.FromBufferBytes(buff);
            }
        }
    }
}
