using System;

namespace Communications.Appi.Buffers
{
    /// <summary>
    /// Указывает на идентификатор данного буфера АППИ
    /// </summary>
    internal class AppiBufferIdentifierAttribute : Attribute
    {
        public AppiBufferIdentifierAttribute(byte Id) { this.Id = Id; }
        public byte Id { get; private set; }
    }
}