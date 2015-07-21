using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    internal class IsoTpFramesPortOptions : PortOptions<IsoTpFrame>
    {
        /// <summary>������ ����� ����� ����� ��� ��������� Loopback</summary>
        /// <param name="SublayerFrameCapacity">������������ ����������� ����� ����������� ������</param>
        public IsoTpFramesPortOptions(int SublayerFrameCapacity)
        {
            this.SublayerFrameCapacity = SublayerFrameCapacity;
        }

        /// <summary>������ ����� ����� ����� � ���������� Loopback � ��������� <see cref="ILoopbackInspector{TFrame}" />
        /// </summary>
        /// <param name="LoopbackInspector">���������� �������� �� Loopback-�����</param>
        /// <param name="SublayerFrameCapacity">������������ ����������� ����� ����������� ������</param>
        public IsoTpFramesPortOptions(ILoopbackInspector<IsoTpFrame> LoopbackInspector, int SublayerFrameCapacity) : base(LoopbackInspector)
        {
            this.SublayerFrameCapacity = SublayerFrameCapacity;
        }

        public int SublayerFrameCapacity { get; private set; }
    }
}