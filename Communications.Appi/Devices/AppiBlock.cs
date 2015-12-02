using Communications.Appi.Decoders;
using Communications.Appi.Encoders;
using Communications.Usb;

namespace Communications.Appi.Devices
{
    public enum AppiLine : byte
    {
        Can1 = 0x02,
        Can2 = 0x03
    }

    public class AppiBlock : AppiDevice<AppiLine>
    {
        public AppiBlock(IUsbDevice UsbDevice, IAppiBufferDecoder Decoder,
                         AppiSendFramesBufferEncoder<AppiLine> SendBufferEncoder,
                         AppiSetBaudRateBufferEncoder<AppiLine> SetBaudRateBufferEncoder)
            : base(UsbDevice, new[] { AppiLine.Can1, AppiLine.Can2 }, Decoder, SendBufferEncoder, SetBaudRateBufferEncoder) { }
    }
}
