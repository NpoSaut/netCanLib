using Communications.Appi.Buffers;
using Communications.Sockets;
using Communications.Usb;

namespace Communications.Appi
{
    /// <summary>Сокет, выполняющий декодирование буферов АППИ из USB-сообщений</summary>
    internal class AppiBufferSocket : ReformativeSocketBase<AppiBuffer, UsbBulk>
    {
        /// <summary>Создаёт экземпляр преобразующего сокета</summary>
        /// <param name="OriginalSocket">Оригинальный сокет, из которого берутся данные</param>
        public AppiBufferSocket(ISocket<UsbBulk> OriginalSocket) : base("USB to APPI Buffer Socket", OriginalSocket) { }

        /// <summary>Выполняет прямое преобразование дейтаграмм источника в выходные дейтаграммы</summary>
        /// <param name="Bulk">Дейтаграмма источника</param>
        /// <returns>Дейтаграмма соответствующего типа</returns>
        protected override AppiBuffer Convert(UsbBulk Bulk) { return AppiBuffer.Decode(Bulk.Data); }

        /// <summary>Выполняет обратное преобразование в тип дейтаграмм источника</summary>
        /// <param name="Buffer">Дейтаграмма</param>
        /// <returns>Дейтаграмма, передаваемая в источник</returns>
        protected override UsbBulk ConvertBack(AppiBuffer Buffer) { return new UsbBulk(Buffer.Encode()); }
    }
}
