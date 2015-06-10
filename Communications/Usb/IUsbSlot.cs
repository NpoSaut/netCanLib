namespace Communications.Usb
{
    public interface IUsbSlot
    {
        IUsbDevice OpenDevice();
    }
}