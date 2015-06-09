using Communications.Appi.Decoders;
using Communications.Appi.Encoders;
using Communications.Usb;

namespace Communications.Appi.Devices
{
    public class AppiBlock : AppiDevice<AppiLine>
    {
        public AppiBlock(IUsbDevice UsbDevice, IAppiBufferDecoder Decoder, AppiSendFramesBufferEncoder<AppiLine> SendBufferEncoder)
            : base(UsbDevice, new[] { AppiLine.Can1, AppiLine.Can2 }, Decoder, SendBufferEncoder) { }
    }
}
