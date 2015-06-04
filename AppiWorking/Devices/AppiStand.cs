using Communications.Appi.Decoders;
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
        public AppiStand(IUsbDevice UsbDevice, IAppiBufferDecoder Decoder)
            : base(UsbDevice, Decoder, new[] { AppiStandLine.CanA, AppiStandLine.CanB, AppiStandLine.CanBusA, AppiStandLine.CanBusB, AppiStandLine.CanTech }) { }

        public AppiStandCanCommutationState CommutationState { get; set; }
    }
}
