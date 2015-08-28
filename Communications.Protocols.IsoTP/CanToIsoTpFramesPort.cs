using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Can;
using Communications.PortHelpers;
using Communications.Protocols.IsoTP.Frames;
using Communications.Transactions;

namespace Communications.Protocols.IsoTP
{
    internal class CanToIsoTpFramesPort : IIsoTpFramesPort
    {
        private readonly ICanPort _canPort;
        private readonly ushort _transmitDescriptor;
        private readonly ushort _receiveDescriptor;
        private readonly IDisposable _rxConnection;

        public CanToIsoTpFramesPort(ICanPort CanPort, ushort TransmitDescriptor, ushort ReceiveDescriptor)
        {
            _canPort = CanPort;
            _transmitDescriptor = TransmitDescriptor;
            _receiveDescriptor = ReceiveDescriptor;
            Options = new IsoTpFramesPortOptions(8, TransmitDescriptor, ReceiveDescriptor);

            IConnectableObservable<InstantaneousTransaction<IsoTpFrame>> rx = CanPort.Rx
                                                           .WaitForTransactionCompleated()
                                                           .Where(f => f.Descriptor == ReceiveDescriptor)
                                                           .Select(f => IsoTpFrame.ParsePacket(f.Data))
                                                           .Select(f => new InstantaneousTransaction<IsoTpFrame>(f))
                                                           .Publish();
            Rx = rx;
            Tx = Observer.Create<IsoTpFrame>(f => CanPort.BeginSend(f.GetCanFrame(TransmitDescriptor)));

            _rxConnection = rx.Connect();
        }

        /// <summary>����� �������� ���������</summary>
        public IObservable<ITransaction<IsoTpFrame>> Rx { get; private set; }

        /// <summary>����� ��������� ���������</summary>
        public IObserver<IsoTpFrame> Tx { get; private set; }

        /// <summary>����� �����</summary>
        public IsoTpFramesPortOptions Options { get; private set; }

        /// <summary>�������� �������� �����</summary>
        /// <param name="Frame">���� ��� ��������</param>
        /// <returns>���������� ��������</returns>
        public ITransaction<IsoTpFrame> BeginSend(IsoTpFrame Frame)
        {
            var canTransmitTransaction = _canPort.BeginSend(Frame.GetCanFrame(_transmitDescriptor));
            return new DecorateTransaction<IsoTpFrame, CanFrame>(Frame, canTransmitTransaction);
        }

        /// <summary>
        ///     ��������� ������������ ����������� ������, ��������� � ���������, �������������� ��� ������� �������������
        ///     ��������.
        /// </summary>
        public void Dispose() { _rxConnection.Dispose(); }
    }
}
