using System.Collections.Generic;
using Communications.Appi.Buffers;

namespace Communications.Appi.Decoders
{
    /// <summary>Композит, выбирающий декодировщик из своего списка, основываясь на типе сообщения</summary>
    /// <remarks>
    ///     Во время конструкции принимает словарь из элементов <see cref="IAppiBufferDecoder" />, сопоставленных с
    ///     числом-идентификатором типа пакета. При вызове метода <see cref="DecodeBuffer" />, смотрит передаваемый ему буфер,
    ///     находит в нём число-идентификатор и пытается найти в своём списке соответствующий декодер; передаёт управление ему.
    /// </remarks>
    public class KeyBasedCompositeBufferDecoder : IAppiBufferDecoder
    {
        private readonly IDictionary<byte, IAppiBufferDecoder> _decoders;

        /// <summary>Создаёт композит, выбирающий декодировщик из своего списка, основываясь на типе сообщения</summary>
        /// <param name="Decoders">Словарь, в котором каждому типу сообщения сопоставлен соответствующий декодер</param>
        public KeyBasedCompositeBufferDecoder(IDictionary<byte, IAppiBufferDecoder> Decoders) { _decoders = Decoders; }

        public Buffer DecodeBuffer(byte[] Buff)
        {
            byte packageType = Buff[0];
            return _decoders[packageType].DecodeBuffer(Buff);
        }
    }
}
