using System;
using System.IO;

namespace Communications.Appi.Encoders
{
    public class AppiSetBaudRateBufferEncoder<TLineKey> : IAppiBufferEncoder<AppiSetBaudRateBuffer<TLineKey>>
    {
        private readonly IInterfaceCodeProvider<TLineKey> _interfaceNumberProvider;
        private readonly AppiSetBaudRateBufferLayout _layout;

        public AppiSetBaudRateBufferEncoder(IInterfaceCodeProvider<TLineKey> InterfaceNumberProvider, AppiSetBaudRateBufferLayout Layout)
        {
            _interfaceNumberProvider = InterfaceNumberProvider;
            _layout = Layout;
        }

        public byte[] Encode(AppiSetBaudRateBuffer<TLineKey> Message)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            writer.BaseStream.Seek(_layout.InterfaceIndexOffset, SeekOrigin.Begin);
            writer.Write(_interfaceNumberProvider.GetInterfaceCode(Message.Interface));

            writer.BaseStream.Seek(_layout.CommandCodeOffset, SeekOrigin.Begin);
            writer.Write((byte)0x01);

            writer.BaseStream.Seek(_layout.BaudRateValueOffset, SeekOrigin.Begin);
            writer.Write((UInt16)(Message.BaudRate / 1000));

            return ms.ToArray();
        }
    }
}
