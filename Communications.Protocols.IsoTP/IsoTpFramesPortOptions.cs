using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    internal class IsoTpFramesPortOptions : PortOptions<IsoTpFrame>
    {
        /// <summary>Создаёт новые опции порта без поддержки Loopback</summary>
        /// <param name="SublayerFrameCapacity">Максимальная вместимость кадра низлежащего уровня</param>
        public IsoTpFramesPortOptions(int SublayerFrameCapacity)
        {
            this.SublayerFrameCapacity = SublayerFrameCapacity;
        }

        /// <summary>Создаёт новые опции порта с поддержкой Loopback и указанным <see cref="ILoopbackInspector{TFrame}" />
        /// </summary>
        /// <param name="LoopbackInspector">Инструмент проверки на Loopback-пакет</param>
        /// <param name="SublayerFrameCapacity">Максимальная вместимость кадра низлежащего уровня</param>
        public IsoTpFramesPortOptions(ILoopbackInspector<IsoTpFrame> LoopbackInspector, int SublayerFrameCapacity) : base(LoopbackInspector)
        {
            this.SublayerFrameCapacity = SublayerFrameCapacity;
        }

        public int SublayerFrameCapacity { get; private set; }
    }
}