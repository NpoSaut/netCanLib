using System;
using Communications.Can;
using Communications.SocketCan.Lib;

namespace Communications.SocketCan.LinuxSockets
{
    /// <summary>Linux Can сокет</summary>
    /// <remarks>Реализует открытие сокета, получение и хранение его номера, а так же запись и чтение по своему номеру сокета</remarks>
    public class LinuxSocket : ILinuxSocket
    {
        private readonly ISocketCanLibFacade _libFacade;
        private readonly int _socketNumber;

        public LinuxSocket(ISocketCanLibFacade LibFacade, string InterfaceName, int RxBuffSize, int TxBuffSize)
        {
            _libFacade = LibFacade;
            RxBufferSize = RxBuffSize;
            TxBufferSize = TxBuffSize;
            _socketNumber = _libFacade.Open(InterfaceName, RxBuffSize, TxBuffSize);
        }

        /// <summary>Выполняет определяемые приложением задачи, связанные с высвобождением или сбросом неуправляемых ресурсов.</summary>
        public void Dispose() { _libFacade.Close(_socketNumber); }

        /// <summary>Размер буфера входящих сообщений</summary>
        public int RxBufferSize { get; private set; }

        /// <summary>Размер буфера исходящих сообщений</summary>
        public int TxBufferSize { get; private set; }

        /// <summary>Смывает буфер входящих сообщений</summary>
        public void FlushInBuffer() { _libFacade.FlushInBuffer(_socketNumber); }

        /// <summary>Ставит сообщения в очередь на отправку в SocketCan</summary>
        /// <param name="Frame">Сообщения для отправки</param>
        public void Send(CanFrame Frame) { _libFacade.Write(_socketNumber, Frame); }

        /// <summary>Выполняет чтение из сокета</summary>
        /// <param name="Timeout">Таймаут операции чтения</param>
        /// <returns>Сообщения, оказавшиеся в буфере чтения на момент вызова процедуры, либо пришедшие за время, указанное в
        ///     <paramref name="Timeout" /> после её вызова.</returns>
        public CanFrame Receive(TimeSpan Timeout) { return _libFacade.Read(_socketNumber, Timeout); }
    }
}
