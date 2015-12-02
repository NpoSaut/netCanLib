using Communications.Appi.Decoders;
using Communications.Appi.Encoders;
using Communications.Usb;

namespace Communications.Appi.Devices
{
    public enum AppiStandLine
    {
        CanA,
        CanB,
        CanBusA,
        CanBusB,
        CanTech
    }

    public enum AppiStandCanCommutationState
    {
        CanBusA,
        CanTech
    }

    public class AppiStand : AppiDevice<AppiStandLine>
    {
        public AppiStand(IUsbDevice UsbDevice, IAppiBufferDecoder Decoder,
                         AppiSendFramesBufferEncoder<AppiStandLine> SendBufferEncoder,
                         AppiSetBaudRateBufferEncoder<AppiStandLine> SetBaudRateEncoder)
            : base(
                UsbDevice, new[] { AppiStandLine.CanA, AppiStandLine.CanB, AppiStandLine.CanBusA, AppiStandLine.CanBusB, AppiStandLine.CanTech }, Decoder,
                SendBufferEncoder, SetBaudRateEncoder) { }

        public AppiStandCanCommutationState CommutationState { get; set; }
    }
}
