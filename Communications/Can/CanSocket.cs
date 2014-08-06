using Communications.Sockets;

namespace Communications.Can
{
    /// <summary>Базовый класс для CAN-сокетов. Предоставляет функционал по фильтрации фреймов по дескрипторам на входе</summary>
    public class CanSocket : BufferedSocketBase<CanFrame>, ICanSocket
    {
        public CanSocket(string Name) : base(Name) { }
        public CanSocket(string Name, IDatagramBuffer<CanFrame> Buffer) : base(Name, Buffer) { }
    }
}
