using System;

namespace Communications.Appi.Buffers
{
    /// <summary>
    /// Указывает на идентефикатор данного буфера АППИ
    /// </summary>
    internal class AppiBufferIdentiferAttribute : Attribute
    {
        public AppiBufferIdentiferAttribute(byte Id) { this.Id = Id; }
        public byte Id { get; private set; }
    }
}