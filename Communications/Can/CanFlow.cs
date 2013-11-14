using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Communications.Can
{
    /// <summary>
    /// Поток CAN-сообщений определённых дескрипторов с определённого порта
    /// </summary>
    /// <remarks>
    /// Поток буферизирует приходящие и исходящие CAN-сообщения, и позволяет отправить CAN-сообщение на порт.
    /// Является полезной обёрткой над CanPort в тех случаях, когда требуется вести диалог через порт.
    /// Позволяет побороться с той проблемой, когда второе устройство отвечает слишком быстро, и программа ещё
    /// не готова дать ответ. При использовании буфера сообщений, каждое принятое сообщение остаётся в потоке
    /// до момента, когда будет программно изъято из него и обработано.
    /// Этот класс похож на класс CanFramesBuffer, но более ориентирован на ведение диалога: он просматривает
    /// лишь один CanPort, но зато позволяет отправлять сообщения.
    /// </remarks>
    public class CanFlow : CanBufferedBase, IDisposable, ICanFlow
    {
        /// <summary>
        /// Порт, через который ведётся диалог
        /// </summary>
        public CanPort Port { get; private set; }

        /// <summary>Массив хэндлеров для отлова интересующих пакетов</summary>
        private List<CanFrameHandler> Handlers = new List<CanFrameHandler>();

        /// <summary>
        /// Список дескрипторов, отлавливаемых в данный поток
        /// </summary>
        public ReadOnlyCollection<int> Descriptors { get; set; }

        /// <summary>
        /// Создаёт поток вокруг выбранного Can-порта
        /// </summary>
        /// <param name="Port">Can-порт, поток с которого нужно буферизировать</param>
        /// <param name="Descriptors">Список обрабатываемых дескрипторов</param>
        public CanFlow(CanPort Port, params int[] Descriptors)
        {
            this.Port = Port;
            this.Descriptors = new ReadOnlyCollection<int>(Descriptors.Distinct().ToList());
            foreach (var d in this.Descriptors)
            {
                var h = new CanFrameHandler(Port, d);
                h.Recieved += new CanFramesReceiveEventHandler(Handler_MessageRecieved);
                Handlers.Add(h);
            }
        }

        void Handler_MessageRecieved(object sender, CanFramesReceiveEventArgs e)
        {
            Enqueue(e.Frames);
        }

        /// <summary>Отправляет Can-сообщение в поток</summary>
        public void Send(CanFrame Frame, bool ClearBeforeSend = false) { Send(new CanFrame[] { Frame }, ClearBeforeSend); }
        /// <summary>Отправляет несколько Can-сообщений в поток</summary>
        public void Send(IList<CanFrame> Frames, bool ClearBeforeSend = false)
        {
            if (ClearBeforeSend) Clear();
            Port.Send(Frames);
        }

        /// <summary>
        /// Закрывает поток, очищая все установленные им хэндлеры
        /// </summary>
        public void Dispose()
        {
            foreach (var h in Handlers)
                h.Dispose();
        }
    }
}
