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
        public AppiStand(IUsbDevice UsbDevice) : base(UsbDevice) { }

        public AppiStandCanCommutationState CommutationState { get; set; }
    }
}
