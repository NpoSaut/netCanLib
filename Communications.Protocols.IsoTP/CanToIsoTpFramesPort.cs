using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP
{
    internal class CanToIsoTpFramesPort : IIsoTpFramesPort
    {
        private readonly IDisposable _rxConnection;

        public CanToIsoTpFramesPort(ICanPort CanPort, ushort TransmitDescriptor, ushort ReceiveDescriptor)
        {
            Options = new IsoTpFramesPortOptions(8);

            IConnectableObservable<IsoTpFrame> rx = CanPort.Rx
                                                           .Where(f => f.Descriptor == ReceiveDescriptor)
                                                           .Select(f => IsoTpFrame.ParsePacket(f.Data))
                                                           .Publish();
            Rx = rx;

            Observer.Create<IsoTpFrame>(f => CanPort.Send(f.GetCanFrame(TransmitDescriptor)),
                                        e => CanPort.Tx.OnError(e));

            _rxConnection = rx.Connect();
        }

        /// <summary>Поток входящих сообщений</summary>
        public IObservable<IsoTpFrame> Rx { get; private set; }

        /// <summary>Поток исходящих сообщений</summary>
        public IObserver<IsoTpFrame> Tx { get; private set; }

        /// <summary>Опции порта</summary>
        public IsoTpFramesPortOptions Options { get; private set; }

        /// <summary>
        ///     Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых
        ///     ресурсов.
        /// </summary>
        public void Dispose()
        {
            _rxConnection.Dispose();
        }
    }
}
