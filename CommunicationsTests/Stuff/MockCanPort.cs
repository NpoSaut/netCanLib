using System.Collections.Generic;
using System.Diagnostics;
using Communications;
using Communications.Can;

namespace CommunicationsTests.Stuff
{
    class MockCanPort : CanPort
    {
        public MockCanPort() : base("Mock Can") { SendBuffer = new Queue<CanFrame>(); }

        /// <summary>
        /// Получает или задаёт скорость порта (в бодах)
        /// </summary>
        public override int BaudRate { get; set; }

        /// <summary>
        /// Внутренняя реализация отправки сообщений
        /// </summary>
        /// <param name="Frames"></param>
        protected override void SendImplementation(IList<CanFrame> Frames)
        {
            foreach (var f in Frames)
                SendBuffer.Enqueue(f);
        }

        protected override ICanSocket CreateSocket() { throw new System.NotImplementedException(); }

        public void PushFrames(params CanFrame[] Frames) { (this as IReceivePipe<CanFrame>).ProcessReceived(Frames); }

        public Queue<CanFrame> SendBuffer { get; private set; }
    }
}