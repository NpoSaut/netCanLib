using System;

namespace Communications.Appi.Buffers
{
    [AppiBufferIdentifer(0x09)]
    class VersionReadAppiBuffer : AppiBufferBase
    {
        public Version AppiVersion { get; set; }
        
        public override byte[] Encode() { throw new System.NotImplementedException(); }

        protected override void DecodeIt(byte[] Buffer) { AppiVersion = new Version(Buffer[6], 0); }
    }
}