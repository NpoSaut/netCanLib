using System;
using System.Collections.Generic;
using Communications;
using Communications.Can;
using Communications.Piping;

namespace SocketCanWorking
{
    public class SocketCanFacade : LoopReadingPipeBase<CanFrame>, ISendPipe<CanFrame>, IReceivePipe<CanFrame>
    {
        /// <summary>Номер сокета</summary>
        public int SocketNumber { get; private set; }

        public SocketCanFacade(String InterfaceName) : base()
        {
            SocketNumber = SocketCanLib.Open(InterfaceName);
            Start();
        }

        /// <summary>
        /// Передаёт дейтаграммы на отправку в SocketCan
        /// </summary>
        public void Send(IList<CanFrame> Frames)
        {
            foreach (var frame in Frames)
            {
                SocketCanLib.Write(SocketNumber, frame);
            }
        }

        /// <summary>
        /// Реализует чтение дейтаграмм из SocketCan
        /// </summary>
        /// <returns></returns>
        protected override IList<CanFrame> ReadDatagrams()
        {
            var frame = SocketCanLib.Read(SocketNumber);
            if (frame != null) return new[] { frame };
            else return new CanFrame[0];
        }

        /// <summary>
        /// Производит закрытие SocketCan-сокета и остановку петли чтения
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            SocketCanLib.Close(SocketNumber);
        }
    }
}
