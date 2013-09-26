using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Communications.Can
{
    /// <summary>
    /// Класс, позволяющий читать сообщения в однопоточном режиме
    /// </summary>
    /// <remarks>
    /// Этот класс организует буфер для приёма сообщений с заданным дескриптором с возможностью блокирующего чтения из этого буфера
    /// </remarks>
    public class CanFramesBuffer : CanBufferedBase, IDisposable
    {
        private List<int> _Descriptors { get; set; }
        /// <summary>
        /// Отслеживаемые дескрипторы
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<int> Descriptors
        {
            get { return _Descriptors.AsReadOnly(); }
        }
        /// <summary>
        /// Порты, сообщения с которых поступают в буфер
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<CanPort> Ports
        {
            get { return Handlers.Select(h => h.Port).Distinct().ToList().AsReadOnly(); }
        }
        private List<CanFrameHandler> Handlers = new List<CanFrameHandler>();

        /// <summary>
        /// Создаёт буфер для отлова сообщений с указанным дескриптором на указанном порту
        /// </summary>
        /// <param name="Descriptor">Отлавливаемый дескриптор</param>
        /// <param name="OnPort"></param>
        public CanFramesBuffer(IEnumerable<int> Descriptors, CanPort OnPort)
            : this(Descriptors, new CanPort[] { OnPort })
        { }
        public CanFramesBuffer(IEnumerable<int> Descriptors, IEnumerable<CanPort> OnPorts)
        {
            this._Descriptors = Descriptors.ToList();
            foreach (var p in OnPorts) RegisterPort(p);
        }
        public CanFramesBuffer(int Descriptor, IEnumerable<CanPort> OnPorts)
            : this(new int[] { Descriptor }, OnPorts)
        { }
        public CanFramesBuffer(int Descriptor, CanPort OnPort)
            : this(new int[] { Descriptor }, new CanPort[] { OnPort })
        { }
        public CanFramesBuffer(CanPort OnPort, params int[] Descriptors)
            : this(Descriptors, OnPort)
        { }

        /// <summary>
        /// Добавляет порт в список прослушиваемых
        /// </summary>
        public void RegisterPort(CanPort Port)
        {
            foreach (var Descriptor in _Descriptors)
            {
                var h = new CanFrameHandler(Port, Descriptor);
                h.Recieved += new CanFramesReceiveEventHandler(Handler_FrameRecieved);
                Handlers.Add(h);
            }
        }

        public void Dispose()
        {
            foreach (var h in Handlers)
                h.Dispose();
        }

        void Handler_FrameRecieved(object sender, CanFramesReceiveEventArgs e)
        {
            Enqueue(e.Frames);
        }
    }
}
