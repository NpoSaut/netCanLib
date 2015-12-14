﻿using System;
using System.IO;
using System.Linq;
using Communications.Can;

namespace Communications.Appi.Encoders
{
    public class AppiSendFramesBufferEncoder<TLineKey> : IAppiBufferEncoder<AppiSendFramesBuffer<TLineKey>>
    {
        private readonly IInterfaceCodeProvider<TLineKey> _interfaceNumberProvider;
        private readonly AppiSendFramesBufferLayout _layout;

        public AppiSendFramesBufferEncoder(IInterfaceCodeProvider<TLineKey> InterfaceNumberProvider, AppiSendFramesBufferLayout Layout)
        {
            _interfaceNumberProvider = InterfaceNumberProvider;
            _layout = Layout;
        }

        public byte[] Encode(AppiSendFramesBuffer<TLineKey> Message)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            writer.BaseStream.Seek(_layout.InterfaceCodeOffset, SeekOrigin.Begin);
            writer.Write(_interfaceNumberProvider.GetInterfaceCode(Message.Interface));

            writer.BaseStream.Seek(_layout.CommandCodeOffset, SeekOrigin.Begin);
            writer.Write((byte)0x02);

            writer.BaseStream.Seek(_layout.FramesCountOffset, SeekOrigin.Begin);
            writer.Write((byte)Message.Frames.Count);

            writer.BaseStream.Seek(_layout.FramesBodyOffset, SeekOrigin.Begin);
            foreach (CanFrame frame in Message.Frames)
            {
                writer.Write(BitConverter.GetBytes((UInt16)frame.Descriptor).Reverse().ToArray());
                writer.Write(frame.Data);
                writer.Seek(8 - frame.Data.Length, SeekOrigin.Current);
            }

            return ms.ToArray();
        }
    }
}