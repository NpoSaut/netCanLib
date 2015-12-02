using Communications.Options;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    internal class IsoTpFramesPortOptions : PortOptions<IsoTpFrame>
    {
        /// <summary>Создаёт новые опции порта без поддержки Loopback</summary>
        /// <param name="SublayerFrameCapacity">Максимальная вместимость кадра низлежащего уровня</param>
        /// <param name="TransmitDescriptor">Дескриптор передаваемых сообщений</param>
        /// <param name="ReceiveDescriptor">Дескриптор принимаемых сообщений</param>
        public IsoTpFramesPortOptions(int SublayerFrameCapacity, ushort TransmitDescriptor, ushort ReceiveDescriptor)
        {
            this.ReceiveDescriptor = ReceiveDescriptor;
            this.TransmitDescriptor = TransmitDescriptor;
            this.SublayerFrameCapacity = SublayerFrameCapacity;
        }

        /// <summary>Создаёт новые опции порта с поддержкой Loopback и указанным <see cref="ILoopbackInspector{TFrame}" />
        /// </summary>
        /// <param name="LoopbackInspector">Инструмент проверки на Loopback-пакет</param>
        /// <param name="SublayerFrameCapacity">Максимальная вместимость кадра низлежащего уровня</param>
        /// <param name="TransmitDescriptor">Дескриптор передаваемых сообщений</param>
        /// <param name="ReceiveDescriptor">Дескриптор принимаемых сообщений</param>
        public IsoTpFramesPortOptions(ILoopbackInspector<IsoTpFrame> LoopbackInspector, int SublayerFrameCapacity, ushort TransmitDescriptor,
                                      ushort ReceiveDescriptor)
            : base(LoopbackInspector)
        {
            this.ReceiveDescriptor = ReceiveDescriptor;
            this.TransmitDescriptor = TransmitDescriptor;
            this.SublayerFrameCapacity = SublayerFrameCapacity;
        }

        public int SublayerFrameCapacity { get; private set; }

        /// <summary>Дескриптор передаваемых сообщений</summary>
        public ushort TransmitDescriptor { get; private set; }

        /// <summary>Дескриптор принимаемых сообщений</summary>
        public ushort ReceiveDescriptor { get; private set; }
    }
}
