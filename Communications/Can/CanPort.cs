using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Communications.Can
{
    /// <summary>
    /// Абстракция CAN-порта
    /// </summary>
    public abstract class CanPort : Port
    {
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
        public event CanFramesReceiveEventHandler Recieved;
        /// <summary>
        /// Генерировать ли Loopback-пакеты для каждого отправленного пакета
        /// </summary>
        public bool GenerateLoopbackEcho { get; set; }

        protected CanPort(String PortName)
            : base(PortName)
        {
            this.GenerateLoopbackEcho = true;
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
            Send(new List<CanFrame>() { Frame });
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
            if (GenerateLoopbackEcho)
                this.OnFramesRecieved(Frames.Select(f => f.GetLoopbackFrame()).ToList());
        }
        #endregion

        /// <summary>
        /// Обработка принятых фреймов
        /// </summary>
        /// <param name="Frames">Принятые фреймы</param>
        protected void OnFramesRecieved(IList<CanFrame> Frames)
        {
            if (Frames.Any())
            {
                foreach (var f in Frames) f.Time = DateTime.Now;

                if (Recieved != null) Recieved(this, new CanFramesReceiveEventArgs(Frames, this));

                lock (_Handlers)
                {
                    foreach (var d in Frames.GroupBy(f => f.Descriptor))
                        foreach (var h in _Handlers.Where(hh => hh.Descriptor == d.Key))
                            h.OnRecieved(d.ToList());
                }
            }
        }
        /// <summary>
        /// Обработка одного принятого фрейма
        /// </summary>
        /// <param name="Frames">Принятый фрейм</param>
        protected void OnFrameRecieved(CanFrame Frame)
        {
            OnFramesRecieved(new List<CanFrame>() { Frame });
        }

        private List<CanFrameHandler> _Handlers = new List<CanFrameHandler>();
        public ReadOnlyCollection<CanFrameHandler> Handlers { get { return _Handlers.ToList().AsReadOnly(); } }
        internal void Handle(CanFrameHandler h)
        {
            lock (_Handlers) _Handlers.Add(h);
        }
        internal void Unandle(CanFrameHandler h)
        {
            lock (_Handlers) _Handlers.Remove(h);
        }

        public override string ToString()
        {
            return string.Format("CanPort {0}", Name);
        }
    }   
    
    public delegate void CanFramesReceiveEventHandler(object sender, CanFramesReceiveEventArgs e);
    public class CanFramesReceiveEventArgs : EventArgs
    {
        public CanPort Port { get; set; }
        public IList<CanFrame> Frames { get; set; }

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
            this.NewBaudRate = NewValue;
        }
    }
}
