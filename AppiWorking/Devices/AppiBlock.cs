using Communications.Appi.Decoders;
using Communications.Usb;

namespace Communications.Appi.Devices
{
    public class AppiBlock : AppiDevice<AppiLine>
    {
        public AppiBlock(IUsbDevice UsbDevice, IAppiBufferDecoder Decoder)
            : base(UsbDevice, Decoder, new[] { AppiLine.Can1, AppiLine.Can2 }) { }
    }
}
