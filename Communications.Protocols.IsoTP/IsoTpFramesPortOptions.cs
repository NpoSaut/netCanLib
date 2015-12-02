using Communications.Options;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    internal class IsoTpFramesPortOptions : PortOptions<IsoTpFrame>
    {
        /// <summary>������ ����� ����� ����� ��� ��������� Loopback</summary>
        /// <param name="SublayerFrameCapacity">������������ ����������� ����� ����������� ������</param>
        /// <param name="TransmitDescriptor">���������� ������������ ���������</param>
        /// <param name="ReceiveDescriptor">���������� ����������� ���������</param>
        public IsoTpFramesPortOptions(int SublayerFrameCapacity, ushort TransmitDescriptor, ushort ReceiveDescriptor)
        {
            this.ReceiveDescriptor = ReceiveDescriptor;
            this.TransmitDescriptor = TransmitDescriptor;
            this.SublayerFrameCapacity = SublayerFrameCapacity;
        }

        /// <summary>������ ����� ����� ����� � ���������� Loopback � ��������� <see cref="ILoopbackInspector{TFrame}" />
        /// </summary>
        /// <param name="LoopbackInspector">���������� �������� �� Loopback-�����</param>
        /// <param name="SublayerFrameCapacity">������������ ����������� ����� ����������� ������</param>
        /// <param name="TransmitDescriptor">���������� ������������ ���������</param>
        /// <param name="ReceiveDescriptor">���������� ����������� ���������</param>
        public IsoTpFramesPortOptions(ILoopbackInspector<IsoTpFrame> LoopbackInspector, int SublayerFrameCapacity, ushort TransmitDescriptor,
                                      ushort ReceiveDescriptor)
            : base(LoopbackInspector)
        {
            this.ReceiveDescriptor = ReceiveDescriptor;
            this.TransmitDescriptor = TransmitDescriptor;
            this.SublayerFrameCapacity = SublayerFrameCapacity;
        }

        public int SublayerFrameCapacity { get; private set; }

        /// <summary>���������� ������������ ���������</summary>
        public ushort TransmitDescriptor { get; private set; }

        /// <summary>���������� ����������� ���������</summary>
        public ushort ReceiveDescriptor { get; private set; }
    }
}
