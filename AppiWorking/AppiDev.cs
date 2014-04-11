using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Communications.Appi.Buffers;
using Communications.Appi.Exceptions;
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
    public abstract class AppiDev : ISocketOwner, IDisposable
    {
        public ILog BufferLog { get; set; }
        private enum BufferDirection { In, Out }
        private void PushBufferToLog(BufferDirection Direction, Byte[] Buffer)
        {
            if (BufferLog != null)
                BufferLog.PushTextEvent(String.Format("{0} {1}", Direction.ToString().PadRight(4), BitConverter.ToString(Buffer).Replace('-', ' ')));
        }

        /// <summary>Размер буфера</summary>
        protected const int BufferSize = 2048;

        private Dictionary<AppiLine, AppiSendPipe> _sendBuffers;

        // Это число, в некотором роде, является волшебным числом.
        // Почему-то, иногда после завершения работы с АППИ, оно начинает выдавать свой буфер с некоторым сдвигом.
        // Тогда размер буфера уменьшается. Но вроде как, он всё равно содержит данные...
        // TODO: надо бы разобраться со сдвигом этого буфера
        public const int MinimumRequiredBufferSize = 524+500;

        protected abstract Byte[] ReadBufferImplement();
        protected abstract void WriteBufferImplement(Byte[] Buffer);

        private Byte[] ReadBuffer()
        {
            lock (_devLocker)
            {
                return ReadBufferImplement();
            }
        }

        internal void WriteBuffer(Byte[] Buffer)
        {
            lock (_devLocker)
            {
                WriteBufferImplement(Buffer);
                unchecked { _sendMessageCounter++; }
            }
        }

        private readonly object _devLocker = new object();

        public AppiRsPort WirelessPort { get; private set; }
        public IDictionary<AppiLine, AppiCanPort> CanPorts { get; private set; }

        private IEnumerable<ISocketOwner> SocketContainers
        {
            get
            {
                foreach (var canPort in CanPorts.Values) yield return canPort;
                yield return WirelessPort;
            }
        }

        public AppiDev()
        {
            CanPorts = new Dictionary<AppiLine, AppiCanPort>()
            {
                { AppiLine.Can1, new AppiCanPort(this, AppiLine.Can1) },
                { AppiLine.Can2, new AppiCanPort(this, AppiLine.Can2) }
            };

            WirelessPort = new AppiRsPort(this, "WRS");

            foreach (var socketContainer in SocketContainers)
                socketContainer.AllSocketsDisposed += SocketContainerOnAllSocketsDisposed;
        }

        private void SocketContainerOnAllSocketsDisposed(object Sender, EventArgs Args)
        {
            if (!HaveOpenedSockets) OnAllSocketsDisposed();
        }

        public virtual void Dispose()
        {
            lock (_devLocker)
            {
                if (IsListening) StopListening();
                foreach (var port in CanPorts.Values)
                {
                    port.Dispose();
                }
                OnDisconnected();
            }
        }

        internal event EventHandler<AppiBufferReadEventArgs> BufferRead;
        private void OnBufferRead(AppiBufferReadEventArgs e)
        {
            EventHandler<AppiBufferReadEventArgs> handler = BufferRead;
            if (handler != null) handler(this, e);
        }

        private static int _lastReadBufferId = -1;
        /// <summary>
        /// Считывает текущее состояние и сообщения из АППИ
        /// </summary>
        /// <returns>Сообщения, полученные с момента предыдущего считывания</returns>
        private void GetAndComputeBuffer()
        {
            var bufferBytes = ReadBuffer();
            PushBufferToLog(BufferDirection.In, bufferBytes);

            var buffer = AppiBufferBase.Decode(bufferBytes);

            // Если принят неопознанный буфер - выходим
            if (buffer == null) return;
            
            // Смотрим, не принимали ли мы это ранее
            if (buffer.SequentNumber == _lastReadBufferId)
            {
                if (BufferLog != null) BufferLog.PushTextEvent("Повторяющийся буфер обнаружен и проигнорирован.");
                return;
            }
            else _lastReadBufferId = buffer.SequentNumber;

            OnBufferRead(new AppiBufferReadEventArgs(buffer));

            if (buffer is MessagesReadAppiBuffer) ProcessMessagesBuffer((MessagesReadAppiBuffer)buffer);
            if (buffer is VersionReadAppiBuffer) ParseVersionBuffer((VersionReadAppiBuffer)buffer);
        }

        private void ProcessMessagesBuffer(MessagesReadAppiBuffer buff)
        {
            OnCanMessagesReceived(buff.CanMessages);
            if (buff.SerialBuffer.Length > 0)
                OnSerialDataReceived(buff.SerialBuffer);
        }

        private readonly object _appiVersionLocker = new object();
        private Version AppiVersion { get; set; }
        private void ParseVersionBuffer(VersionReadAppiBuffer buff)
        {
            lock (_appiVersionLocker)
            {
                AppiVersion = buff.AppiVersion;
                Monitor.PulseAll(_appiVersionLocker);
            }
        }

        internal Version GetAppiVersion()
        {
            var versionAskingBuffer = new byte[] {0x09, 0x01};
            bool versionReceived;
            lock (_appiVersionLocker)
            {
                AppiVersion = null;
                WriteBuffer(versionAskingBuffer);
                versionReceived = Monitor.Wait(_appiVersionLocker, TimeSpan.FromSeconds(2));
            }
            if (!versionReceived)
            {
                OnDisconnected();
                throw new AppiConnectoinException("АППИ не ответило на запрос версии. Скорее всего, не удаётся установить связь с АППИ.");
            }
            return AppiVersion;
        }

        private void OnSerialDataReceived(byte[] serialData)
        {
            (WirelessPort as IReceivePipe<Byte>).ProcessReceived(serialData);
        }

        public const int FramesPerSendGroup = 20;
        private byte _sendMessageCounter = 0;
        /// <summary>
        /// Отправляет список сообщений в указанный канал
        /// </summary>
        /// <param name="Frames">Список сообщений</param>
        /// <param name="Line">Канал связи</param>
        internal void SendFrames(IList<CanFrame> Frames, AppiLine Line)
        {
            if (_sendBuffers == null) throw new AppiException("Не инициализированы средства отправки в CAN-линию");
            if (!_sendBuffers.ContainsKey(Line)) throw new AppiException("Не инициализировано средства отправки в линию {0}", Line);
            
            _sendBuffers[Line].SynchronizedSend(Frames);
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

                var ms = new MemoryStream(2048);
                ms.WriteByte(0x01);
                ms.WriteByte(0x02);
                ms.Seek(8, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(len), 0, 2);
                ms.Write(buff, pointer, len);

                var usbBuff = ms.GetBuffer();
                WriteBuffer(usbBuff);
                PushBufferToLog(BufferDirection.Out, usbBuff);
            }
        }

        internal event AppiReceiveEventHandler AppiMessagesReceived;

        private void OnCanMessagesReceived(IDictionary<AppiLine, IList<CanFrame>> messages)
        {
            if (AppiMessagesReceived != null) AppiMessagesReceived(this, new AppiMessageReceiveEventArgs(messages));

            foreach (var kvp in messages.Where(kvp => kvp.Value.Any()))
            {
                (CanPorts[kvp.Key] as IReceivePipe<CanFrame>).ProcessReceived(kvp.Value);
            }
        }

        /// <summary>
        /// Признак действия режима прослушивания линии
        /// </summary>
        private bool IsListening { get; set; }
        private Thread _listeningThread;
        /// <summary>
        /// Начать прослушивание линии
        /// </summary>
        /// <remarks>Запускает отдельный поток для прослушивания линии</remarks>
        private void BeginListen()
        {
            lock (_devLocker)
                if (!IsListening)
                {
                    _listeningThread = new Thread(ListeningLoop) { Name = "Поток прослушивания АППИ" };
                    IsListening = true;
                    _listeningThread.Start();
                }
        }
        /// <summary>
        /// Петля прослушивания линии
        /// </summary>
        private void ListeningLoop()
        {
            while (true)
            {
                try
                {
                    if (!IsListening) break;
                    else this.GetAndComputeBuffer();
                    Thread.Sleep(1);
                }
                catch (AppiConnectoinException)
                {
                    this.OnDisconnected();
                    break;
                }
            }
        }
        /// <summary>
        /// Остановить прослушивание линии
        /// </summary>
        private void StopListening()
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

        private bool _disconnectionProcessed = false;
        /// <summary>
        /// События при отключении устройства.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            lock (_devLocker)
            {
                if (!_disconnectionProcessed)
                {
//                    lock (_appiVersionLocker)
//                    {
//                        Monitor.PulseAll(_appiVersionLocker);
//                    }
                    if (_sendBuffers != null)
                        foreach (var appiSendBuffer in _sendBuffers.Values)
                        {
                            appiSendBuffer.AbortAllTransfers();
                        }
                    _disconnectionProcessed = true;
                    if (Disconnected != null) Disconnected(this, new EventArgs());
                }
            }
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

        internal void Initialize()
        {
            BeginListen();
            GetAppiVersion();
            if (AppiVersion < new Version(4, 0))
            {
                _sendBuffers =
                    new Dictionary<AppiLine, AppiSendPipe>
                    {
                        {AppiLine.Can1, new AppiTimeoutSendPipe(this, AppiLine.Can1)},
                        {AppiLine.Can2, new AppiTimeoutSendPipe(this, AppiLine.Can2)}
                    };
            }
            else
            {
                _sendBuffers =
                    new Dictionary<AppiLine, AppiSendPipe>
                    {
                        {AppiLine.Can1, new AppiFeedbackSendPipe(this, AppiLine.Can1)},
                        {AppiLine.Can2, new AppiFeedbackSendPipe(this, AppiLine.Can2)}
                    };
            }
        }

        public event EventHandler AllSocketsDisposed;
        private void OnAllSocketsDisposed()
        {
            var handler = AllSocketsDisposed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public bool HaveOpenedSockets { get { return SocketContainers.Any(c => c.HaveOpenedSockets); } }
    }

    public delegate void AppiReceiveEventHandler(object sender, AppiMessageReceiveEventArgs e);
    public class AppiMessageReceiveEventArgs : EventArgs
    {
        public IDictionary<AppiLine, IList<CanFrame>> Messages { get; set; }

        public AppiMessageReceiveEventArgs(IDictionary<AppiLine, IList<CanFrame>> Messages)
        {
            this.Messages = Messages;
        }
    }

    internal class AppiBufferReadEventArgs : EventArgs
    {
        public AppiBufferBase Buffer { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.EventArgs"/>.
        /// </summary>
        public AppiBufferReadEventArgs(AppiBufferBase Buffer) { this.Buffer = Buffer; }
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
