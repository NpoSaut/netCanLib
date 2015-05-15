using Communications.Appi.Decoders;

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
        public AppiStand(IUsbDevice UsbDevice, IAppiBufferDecoder Decoder) : base(UsbDevice, Decoder) { }
        public AppiStandCanCommutationState CommutationState { get; set; }
    }
}
