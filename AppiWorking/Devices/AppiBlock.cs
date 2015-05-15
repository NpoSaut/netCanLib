using System.Collections.Generic;
using Communications.Appi.Decoders;

namespace Communications.Appi.Devices
{
    public class AppiBlock : AppiDevice<AppiLine>
    {
        public AppiBlock(IUsbDevice UsbDevice, IAppiBufferDecoder Decoder)
            : base(UsbDevice, Decoder)
        {
            CanPorts = new Dictionary<AppiLine, AppiCanPort>
                            {
                                { AppiLine.Can1, new AppiCanPort(this, AppiLine.Can1) }
                            };
        }
    }
}
