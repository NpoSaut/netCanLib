using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using NLog;

namespace Communications.Can
{
    /// <summary>
    /// Абстракция CAN-порта
    /// </summary>
    public abstract class CanPort : Port
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Получает или задаёт скорость порта (в бодах)
        /// </summary>
        public abstract int BaudRate { get; set; }
        public event BaudRateChangedEventHandler BaudRateChanged;
        protected void OnBaudRateChanged(int newValue)
        {
            if (BaudRateChanged != null) BaudRateChanged(this, new BaudRateChangedEventArgs(newValue));
        }

        /// <summary>
        /// Событие приёма сообщения по линии
        /// </summary>
        public event CanFramesReceiveEventHandler Received;
        /// <summary>
        /// Генерировать ли Loopback-пакеты для каждого отправленного пакета
        /// </summary>
        public bool GenerateLoopbackEcho { get; set; }

        protected CanPort(String PortName)
            : base(PortName)
        {
            GenerateLoopbackEcho = true;
            _logger = LogManager.GetLogger(PortName);
        }

        #region Отправка сообщений
        /// <summary>
        /// Отправка нескольких фреймов в линию
        /// </summary>
        /// <param name="Frames">Фреймы для отправки</param>
        public void Send(IList<CanFrame> Frames)
        {
            SendImplementation(Frames);
            OnSent(Frames);
        }
        /// <summary>
        /// Отправка одного фрейма в линию
        /// </summary>
        /// <param name="Frame">Фрейм для отправки</param>
        public void Send(CanFrame Frame)
        {
            Send(new List<CanFrame> { Frame });
        }
        /// <summary>
        /// Внутренняя реализация отправки сообщений
        /// </summary>
        /// <param name="Frames"></param>
        protected abstract void SendImplementation(IList<CanFrame> Frames);
        /// <summary>
        /// Обрабатывается после отправки сообщений
        /// </summary>
        /// <param name="Frames">Список отправленных сообщений</param>
        protected virtual void OnSent(IList<CanFrame> Frames)
        {
            LogFramesFlow(FramesFlowDirection.Out, Frames);
            if (GenerateLoopbackEcho)
                OnFramesReceived(Frames.Select(f => f.GetLoopbackFrame()).ToList());
        }
        #endregion

        /// <summary>
        /// Обработка принятых фреймов
        /// </summary>
        /// <param name="Frames">Принятые фреймы</param>
        protected void OnFramesReceived(IList<CanFrame> Frames)
        {
            if (Frames.Any())
            {
                foreach (var f in Frames) f.Time = DateTime.Now;

                LogFramesFlow(FramesFlowDirection.In, Frames);

                if (Received != null) Received(this, new CanFramesReceiveEventArgs(Frames, this));

                lock (_handlers)
                {
                    foreach (var d in Frames.GroupBy(f => f.Descriptor))
                        foreach (var h in _handlers.Where(hh => hh.Descriptor == d.Key))
                            h.OnReceived(d.ToList());
                }
            }
        }
        /// <summary>
        /// Обработка одного принятого фрейма
        /// </summary>
        /// <param name="Frame">Принятый фрейм</param>
        protected void OnFrameReceived(CanFrame Frame)
        {
            OnFramesReceived(new List<CanFrame> { Frame });
        }

        private readonly List<CanFrameHandler> _handlers = new List<CanFrameHandler>();
        public ReadOnlyCollection<CanFrameHandler> Handlers { get { return _handlers.ToList().AsReadOnly(); } }
        internal void Handle(CanFrameHandler h)
        {
            lock (_handlers) _handlers.Add(h);
        }
        internal void UnHandle(CanFrameHandler h)
        {
            lock (_handlers) _handlers.Remove(h);
        }

        public override string ToString()
        {
            return string.Format("CanPort {0}", Name);
        }

        private enum FramesFlowDirection { In, Out }

        private void LogFramesFlow(FramesFlowDirection Direction, IList<CanFrame> Frames)
        {
            foreach (CanFrame frame in Frames)
                _logger.Trace("{0,4} {1}", Direction, frame);
        }
    }   
    
    public delegate void CanFramesReceiveEventHandler(object sender, CanFramesReceiveEventArgs e);
    public class CanFramesReceiveEventArgs : EventArgs
    {
        public CanPort Port { get; private set; }
        public IList<CanFrame> Frames { get; private set; }

        public CanFramesReceiveEventArgs(IList<CanFrame> Frames, CanPort Port)
        {
            this.Frames = Frames;
            this.Port = Port;
        }
    }

    public delegate void BaudRateChangedEventHandler(object sender, BaudRateChangedEventArgs e);
    public class BaudRateChangedEventArgs : EventArgs
    {
        public int NewBaudRate { get; private set; }

        public BaudRateChangedEventArgs(int NewValue)
        {
            NewBaudRate = NewValue;
        }
    }
}
