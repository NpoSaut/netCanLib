using System;

namespace Communications.Appi.Buffers
{
    [AppiBufferIdentifer(0x09)]
    public class VersionReadAppiBuffer : Buffer
    {
        public VersionReadAppiBuffer(int SequentialNumber, Version AppiVersion) : base(SequentialNumber) { this.AppiVersion = AppiVersion; }
        public Version AppiVersion { get; private set; }
    }
}
