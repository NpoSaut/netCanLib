using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly List<int> _descriptors;
        /// <summary>
        /// Отслеживаемые дескрипторы
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<int> Descriptors
        {
            get { return _descriptors.AsReadOnly(); }
        }
        /// <summary>
        /// Порты, сообщения с которых поступают в буфер
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<CanPort> Ports
        {
            get { return _handlers.Select(h => h.Port).Distinct().ToList().AsReadOnly(); }
        }
        private readonly List<CanFrameHandler> _handlers = new List<CanFrameHandler>();

        /// <summary>
        /// Создаёт буфер для отлова сообщений с указанным дескриптором на указанном порту
        /// </summary>
        /// <param name="Descriptors">Отлавливаемые дескрипторы</param>
        /// <param name="OnPort"></param>
        public CanFramesBuffer(IEnumerable<int> Descriptors, CanPort OnPort)
            : this(Descriptors, new[] { OnPort })
        { }
        public CanFramesBuffer(IEnumerable<int> Descriptors, IEnumerable<CanPort> OnPorts)
        {
            _descriptors = Descriptors.ToList();
            foreach (var p in OnPorts) RegisterPort(p);
        }
        public CanFramesBuffer(int Descriptor, IEnumerable<CanPort> OnPorts)
            : this(new[] { Descriptor }, OnPorts)
        { }
        public CanFramesBuffer(int Descriptor, CanPort OnPort)
            : this(new[] { Descriptor }, new[] { OnPort })
        { }
        public CanFramesBuffer(CanPort OnPort, params int[] Descriptors)
            : this(Descriptors, OnPort)
        { }

        /// <summary>
        /// Добавляет порт в список прослушиваемых
        /// </summary>
        public void RegisterPort(CanPort Port)
        {
            foreach (var descriptor in _descriptors)
            {
                var h = new CanFrameHandler(Port, descriptor);
                h.Received += Handler_FrameReceived;
                _handlers.Add(h);
            }
        }

        public void Dispose()
        {
            foreach (var h in _handlers)
                h.Dispose();
        }

        void Handler_FrameReceived(object sender, CanFramesReceiveEventArgs e)
        {
            Enqueue(e.Frames);
        }
    }
}
