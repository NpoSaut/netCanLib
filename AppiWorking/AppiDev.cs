using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;
using System.IO;

namespace Communications.Appi
{
    public enum AppiLine : byte
    { 
        Can1 = 0x02,
        Can2 = 0x03
    }

    /// <summary>
    /// Представление АППИ
    /// </summary>
    public abstract class AppiDev : IDisposable
    {
        public Communications.ILog BufferLog { get; set; }
        private enum BufferDirection { In, Out }
        private void PushBufferToLog(BufferDirection Direction, Byte[] Buffer)
        {
            if (BufferLog != null)
                BufferLog.PushTextEvent(string.Format("{0} {1}", Direction.ToString().PadRight(4), BitConverter.ToString(Buffer).Replace('-', ' ')));
        }

        /// <summary>
        /// Размер буфера
        /// </summary>
        public const int BufferSize = 2048;

        // Это число, в некотором роде, является волшебным числом.
        // Почему-то, иногда после завершения работы с АППИ, оно начинает выдавать свой буфер с некоторым сдвигом.
        // Тогда размер буфера уменьшается. Но вроде как, он всё равно содержит данные...
        // TODO: надо бы разобраться со сдвигом этого буфера
        public const int MinimumRequiredBufferSize = 524+500;

        protected abstract Byte[] ReadBuffer();
        protected abstract void WriteBuffer(Byte[] Buffer);

        private object DevLocker = new object();

        public AppiRsPort WirelessPort { get; private set; }
        public IDictionary<AppiLine, AppiCanPort> CanPorts { get; private set; }

        public AppiDev()
        {
            CanPorts = new Dictionary<AppiLine, AppiCanPort>()
            {
                { AppiLine.Can1, new AppiCanPort(this, AppiLine.Can1) },
                { AppiLine.Can2, new AppiCanPort(this, AppiLine.Can2) }
            };
            WirelessPort = new AppiRsPort(this, "WRS");
        }
        public virtual void Dispose()
        {
            lock (DevLocker)
            {
                if (IsListening)
                    StopListening();
            }
        }

        private static int LastReadBufferId = -1;
        /// <summary>
        /// Считывает текущее состояние и сообщения из АППИ
        /// </summary>
        /// <returns>Сообщения, полученные с момента предыдущего считывания</returns>
        private void ReadMessages()
        {
            Byte[] buff;
            lock (DevLocker)
            {
                buff = ReadBuffer();
                PushBufferToLog(BufferDirection.In, buff);
            }

            if (buff.Length < MinimumRequiredBufferSize)
            {
                Console.Write('~');
            }

            // Смотрим, не принимали ли мы это ранее
            //Console.WriteLine("{0} -> {1}   | {2} : {3}  | {4}", LastReadBufferId, buff[5], buff[6], buff[2], string.Join(" ", buff.Take(10).Select((i,b) => string.Format("{0}:{1:X2}", b, i))));
            if (buff[5] == LastReadBufferId)
            {
                //Console.WriteLine("DUBLICATE!");
                if (BufferLog != null) BufferLog.PushTextEvent("Повторяющийся буфер обнаружен и проигнорирован.");
                return;
            }
            else LastReadBufferId = buff[5];

            var MessagesInA = buff[6];
            var MessagesInB = buff[2];

            int SpeedA = buff[7] | buff[8] << 8;
            int SpeedB = buff[9] | buff[10] << 8;

            CanPorts[AppiLine.Can1].RenewBaudRate(SpeedA * 1000);
            CanPorts[AppiLine.Can2].RenewBaudRate(SpeedB * 1000);

            var messages = new AppiMessages(
                    ParseBuffer(buff, 24, MessagesInA).ToList(),
                    ParseBuffer(buff, 524, MessagesInB).ToList()
                    );
            OnMessagesRecieved(messages);

            var BytesInSerial = buff[1024];
            if (BytesInSerial > 0)
            {
                byte[] serialData = new byte[BytesInSerial];
                Buffer.BlockCopy(buff, 1025, serialData, 0, serialData.Length);
                OnSerialDataRecieved(serialData);
            }
        }

        private void OnSerialDataRecieved(byte[] serialData)
        {
            WirelessPort.OnAppiRsBufferRead(serialData);
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
                Buffer.BlockCopy(Buff, Offset + i * buff.Length, buff, 0, buff.Length);
                yield return AppiCanFrameConstructor.FromBufferBytes(buff);
            }
        }

        public const int FramesPerSendGroup = 20;
        private byte SendMessageCounter = 0;
        /// <summary>
        /// Отправляет список сообщений в указанный канал
        /// </summary>
        /// <param name="Frames">Список сообщений</param>
        /// <param name="Line">Канал связи</param>
        internal void SendFrames(IList<CanFrame> Frames, AppiLine Line)
        {
            var FrameGroups = Frames
                    .Select((f, i) => new { f, i })
                    .GroupBy(fi => fi.i / FramesPerSendGroup, fi => fi.f)
                    .Select(fg => fg.ToList())
                    .ToList();

            foreach (var fg in FrameGroups)
            {
                lock (DevLocker)
                {
                    Byte[] Buff = new Byte[2048];
                    Buffer.SetByte(Buff, 0, 0x02);
                    Buffer.SetByte(Buff, 1, (byte)Line);
                    Buffer.SetByte(Buff, 3, SendMessageCounter);
                    Buffer.SetByte(Buff, 3, (byte)fg.Count);

                    var MessagesBuffer = fg.SelectMany(m => m.ToBufferBytes()).ToArray();
                    Buffer.BlockCopy(MessagesBuffer, 0, Buff, 10, MessagesBuffer.Length);

                    WriteBuffer(Buff);
                    System.Threading.Thread.Sleep(fg.Count);
                    PushBufferToLog(BufferDirection.Out, Buff);

                    unchecked { SendMessageCounter++; }
                }
            }
        }
        /// <summary>
        /// Отправляет одно сообщение в канал
        /// </summary>
        /// <param name="Frame">CAN-Сообщение</param>
        /// <param name="Line">Канал связи</param>
        internal void SendFrame(CanFrame Frame, AppiLine Line)
        {
            SendFrames(new List<CanFrame>() { Frame }, Line);
        }

        internal void PushSerialData(byte[] buff)
        {
            for (int pointer = 0; pointer < buff.Length; pointer += UInt16.MaxValue)
            {
                UInt16 len = (UInt16)Math.Min(buff.Length - pointer, UInt16.MaxValue);

                MemoryStream ms = new MemoryStream(2048);
                ms.WriteByte(0x01);
                ms.WriteByte(0x02);
                ms.Seek(8, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(len), 0, 2);
                ms.Write(buff, pointer, len);

                lock (DevLocker)
                {
                    var UsbBuff = ms.GetBuffer();
                    WriteBuffer(UsbBuff);
                    PushBufferToLog(BufferDirection.Out, UsbBuff);
                }
            }
        }

        public event AppiReceiveEventHandler AppiMessagesRecieved;

        private void OnMessagesRecieved(AppiMessages mes)
        {
            if (AppiMessagesRecieved != null) AppiMessagesRecieved(this, new AppiMessageRecieveEventArgs(mes));

            CanPorts[AppiLine.Can1].OnAppiFramesRecieved(mes.ChannelA);
            CanPorts[AppiLine.Can2].OnAppiFramesRecieved(mes.ChannelB);
        }

        /// <summary>
        /// Признак действия режима прослушивания линии
        /// </summary>
        public bool IsListening { get; private set; }
        private System.Threading.Thread ListeningThread;
        /// <summary>
        /// Начать прослушивание линии
        /// </summary>
        /// <remarks>Запускает отдельный поток для прослушивания линии</remarks>
        public void BeginListen()
        {
            lock (DevLocker)
                if (!IsListening)
                {
                    ListeningThread = new System.Threading.Thread(ListeningLoop) { Name = "Поток прослушивания АППИ" };
                    IsListening = true;
                    ListeningThread.Start();
                }
        }
        /// <summary>
        /// Петля прослушивания линии
        /// </summary>
        private void ListeningLoop()
        {
            while (true)
            {
                lock (DevLocker)
                {
                    try
                    {
                        if (!IsListening) break;
                        else this.ReadMessages();
                        System.Threading.Thread.Sleep(1);
                    }
                    catch (AppiConnectoinException)
                    {
                        this.OnDisconnected();
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Остановить прослушивание линии
        /// </summary>
        public void StopListening()
        {
            if (IsListening)
            {
                IsListening = false;
                //ListeningThread.Abort();
            }
        }

        /// <summary>
        /// Возникает при отключении устройства
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// События при отключении устройства.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            if (Disconnected != null) Disconnected(this, new EventArgs());
        }

        /// <summary>
        /// Устанавливает скорость обмена по выбранной линии
        /// </summary>
        /// <param name="appiLine">Линия, для которой требуется установить скорость</param>
        /// <param name="value">Новое значение скорости</param>
        internal void SetBaudRate(AppiLine appiLine, int value)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write((byte)appiLine);
                    bw.Write((byte)0x01);
                    bw.Write((UInt16)(value / 1000));
                }
                var buff = ms.ToArray();
                WriteBuffer(buff);
                PushBufferToLog(BufferDirection.Out, buff);
            }
            
        }
    }

    public delegate void AppiReceiveEventHandler(object sender, AppiMessageRecieveEventArgs e);
    public class AppiMessageRecieveEventArgs : EventArgs
    {
        public AppiMessages Messages { get; set; }

        public AppiMessageRecieveEventArgs(AppiMessages Messages)
        {
            this.Messages = Messages;
        }
    }

    internal static class AppiCanFrameConstructor
    {
        /// <summary>
        /// Собирает байты для буфера АППИ
        /// </summary>
        /// <returns>10 байт: 2 байта дескриптор + 8 байт данных</returns>
        public static Byte[] ToBufferBytes(this CanFrame Frame)
        {
            Byte[] buff = new Byte[10];

            BitConverter.GetBytes((UInt16)Frame.Descriptor).Reverse().ToArray().CopyTo(buff, 0);
            Frame.Data.CopyTo(buff, 2);

            return buff;
        }

        /// <summary>
        /// Восстанавливает CAN-сообщение из буфера АППИ
        /// </summary>
        /// <param name="Buff">10 байт буфера</param>
        public static CanFrame FromBufferBytes(Byte[] Buff)
        {
            int id = (int)BitConverter.ToUInt16(Buff.Take(2).Reverse().ToArray(), 0) >> 4;
            int len = Buff[1] & 0x0f;
            if (len > 8) throw new AppiBufferDecodeException("Расшифрована неправильная длина CAN-сообщения ({0} байт)", len);
            return CanFrame.NewWithId(id, Buff, 2, len);
        }
    }
}
