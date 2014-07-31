using System;
using System.Collections.Generic;
using Communications;
using Communications.Can;
using Communications.Piping;

namespace SocketCanWorking
{
    public class SocketCanFacade : LoopReadingPipeBase<CanFrame>, ISendPipe<CanFrame>, IReceivePipe<CanFrame>
    {
        public SocketCanFacade(String InterfaceName)
        {
            SocketNumber = SocketCanLib.Open(InterfaceName);
            Start();
        }

        /// <summary>Номер сокета.</summary>
        public int SocketNumber { get; private set; }

        /// <summary>Передаёт дейтаграммы на отправку в SocketCan.</summary>
        public void Send(IList<CanFrame> Frames)
        {
            foreach (CanFrame frame in Frames)
                SocketCanLib.Write(SocketNumber, frame);
        }

        /// <summary>Реализует чтение дейтаграмм из SocketCan.</summary>
        protected override IList<CanFrame> ReadDatagrams()
        {
            CanFrame frame = SocketCanLib.Read(SocketNumber);
            if (frame != null) return new[] { frame };
            return new CanFrame[0];
        }

        /// <summary>Производит закрытие SocketCan-сокета и остановку петли чтения.</summary>
        public override void Dispose()
        {
            base.Dispose();
            SocketCanLib.Close(SocketNumber);
        }
    }
}
