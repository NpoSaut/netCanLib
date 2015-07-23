using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Syntax;
using Communications.Can;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.Transactions;

namespace Communications.Protocols.IsoTP
{
    internal enum IsoTpState
    {
        ReadyToReceive,
        Receiving,
        Transmiting
    }

    internal enum IsoTpEvent
    {
        TransmitRequest,
        Abort,
        Timeout,
        FrameReceived,
        PackageReceived,
        PackageSent
    }

    public class IsoTpOverCanPort : IIsoTpConnection
    {
        private readonly IsoTpConnectionParameters _connectionParameters;
        private readonly IStateMachine<IsoTpState, IsoTpEvent> _fsm;
        private readonly IIsoTpFramesPort _port;
        private readonly Subject<IsoTpPacket> _rx;
        private readonly IDisposable _rxFromBelowConnection;
        private readonly IScheduler _scheduler;

        private ReceiveTransaction _receiveTransaction;
        private IDisposable _timeoutToken;
        private TransmitTransaction _transmitTransaction;

        public IsoTpOverCanPort(ICanPort CanPort, ushort TransmitDescriptor, ushort ReceiveDescriptor, IsoTpConnectionParameters ConnectionParameters)
        {
            Options = new IsoTpOverCanPortOptions(TransmitDescriptor, ReceiveDescriptor);
            _connectionParameters = ConnectionParameters;
            _scheduler = new EventLoopScheduler();

            _port = new CanToIsoTpFramesPort(CanPort, TransmitDescriptor, ReceiveDescriptor);

            _rx = new Subject<IsoTpPacket>();

            _fsm = new PassiveStateMachine<IsoTpState, IsoTpEvent>();
            _fsm.Initialize(IsoTpState.ReadyToReceive);
            ConfigureStateMachine();

            _rxFromBelowConnection =
                _port.Rx
                     .SubscribeOn(_scheduler)
                     .Subscribe(f => _fsm.Fire(IsoTpEvent.FrameReceived, f));

            Tx = Observer.Create<IsoTpPacket>(TransmitPacket,
                                              e => _fsm.Fire(IsoTpEvent.Abort));

            _fsm.Start();
        }

        private void TransmitPacket(IsoTpPacket p)
        {
            _fsm.Fire(IsoTpEvent.TransmitRequest, p);
        }

        public void Dispose()
        {
            _fsm.Stop();
            _rxFromBelowConnection.Dispose();
            _rx.Dispose();
            _port.Dispose();
        }

        private void ConfigureStateMachine()
        {
            #region Ready To Receive

            _fsm.In(IsoTpState.ReadyToReceive)
                .ExecuteOnEntry(DecockTimer);

            _fsm.In(IsoTpState.ReadyToReceive)
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is SingleFrame)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute<SingleFrame>(WhenSingleFrameComes);

            _fsm.In(IsoTpState.ReadyToReceive)
                .On(IsoTpEvent.PackageReceived)
                .Execute<IsoTpPacket>(p => _rx.OnNext(p));

            _fsm.In(IsoTpState.ReadyToReceive)
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is FirstFrame)
                .Goto(IsoTpState.Receiving)
                .Execute<FirstFrame>(WhenFirstFrameComes)
                .Execute(() => CockTimer(_connectionParameters.ConsecutiveTimeout));

            _fsm.In(IsoTpState.ReadyToReceive)
                .On(IsoTpEvent.TransmitRequest)
                // Маленький пакет
                .If<IsoTpPacket>(p => p.Data.Length <= SingleFrame.GetPayload(_port.Options.SublayerFrameCapacity))
                .Execute<IsoTpPacket>(p => _port.Send(new SingleFrame(p.Data)))
                // Большой пакет
                .Otherwise()
                .Goto(IsoTpState.Transmiting)
                .Execute<IsoTpPacket>(BeginTransmitTransaction)
                .Execute(() => CockTimer(_connectionParameters.FirstResponseTimeout));

            #endregion

            #region Receiving

            IEntryActionSyntax<IsoTpState, IsoTpEvent> whenReceiving = _fsm.In(IsoTpState.Receiving);

            whenReceiving
                .On(IsoTpEvent.FrameReceived)
                // Правильный пакет с данными
                .If<IsoTpFrame>(IsExpectedConsecutiveData)
                .Execute<ConsecutiveFrame>(WhenConsecutiveDataComes)
                .Execute(() => CockTimer(_connectionParameters.ConsecutiveTimeout))
                // Другой пакет с данными
                .If<IsoTpFrame>(f => f is ConsecutiveFrame)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction)
                .Execute<ConsecutiveFrame>(f => { throw new IsoTpSequenceException(_receiveTransaction.ExpectedCounter, f); })
                // Новая короткая транзакция
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is SingleFrame)
                .Execute(() => { throw new IsoTpTransactionLostException(); })
                .Execute<SingleFrame>(WhenSingleFrameComes)
                // Новая длинная транзакция
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is FirstFrame)
                .Goto(IsoTpState.Receiving)
                .Execute(() => { throw new IsoTpTransactionLostException(); })
                .Execute<FirstFrame>(WhenFirstFrameComes)
                .Execute(() => CockTimer(_connectionParameters.ConsecutiveTimeout));

            whenReceiving
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is ConsecutiveFrame)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction)
                .Execute<IsoTpFrame>(f => { throw new IsoTpWrongFrameException(f, typeof (ConsecutiveFrame)); });

            whenReceiving
                .On(IsoTpEvent.Timeout)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction)
                .Execute(() => { throw new IsoTpTimeoutException(); });

            whenReceiving
                .On(IsoTpEvent.Abort)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction);

            whenReceiving
                .On(IsoTpEvent.PackageReceived)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute<IsoTpPacket>(p => _rx.OnNext(p))
                .Execute(() => _receiveTransaction = null);

            whenReceiving
                .On(IsoTpEvent.TransmitRequest)
                .Execute(() => { throw new IsoTpPortIsBusyException(); });

            #endregion

            #region Transmiting

            IEntryActionSyntax<IsoTpState, IsoTpEvent> whenTransmiting = _fsm.In(IsoTpState.Transmiting);

            whenTransmiting
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is FlowControlFrame && ((FlowControlFrame)f).Flag == FlowControlFlag.ClearToSend)
                .Execute<FlowControlFrame>(SendNextDataPortion)
                .Execute(() => CockTimer(_connectionParameters.ConsecutiveTimeout))
                .If<IsoTpFrame>(f => f is FlowControlFrame && ((FlowControlFrame)f).Flag == FlowControlFlag.Wait)
                .Execute(() => CockTimer(_connectionParameters.ConsecutiveTimeout))
                .If<IsoTpFrame>(f => f is FlowControlFrame && ((FlowControlFrame)f).Flag == FlowControlFlag.Abort)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(() => { throw new IsoTpTransactionAbortedException(); })
                .Otherwise()
                .Execute<IsoTpFrame>(f => { throw new IsoTpWrongFrameException(f, typeof (FlowControlFrame)); });

            whenTransmiting
                .On(IsoTpEvent.Timeout)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(() => { throw new IsoTpTimeoutException(); });

            whenTransmiting
                .On(IsoTpEvent.Abort)
                .Goto(IsoTpState.ReadyToReceive);

            whenTransmiting
                .On(IsoTpEvent.TransmitRequest)
                .Execute(() => { throw new IsoTpPortIsBusyException(); });

            whenTransmiting
                .On(IsoTpEvent.PackageSent)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(() => _transmitTransaction.Submit());

            #endregion
        }

        private void CockTimer(TimeSpan Timeout)
        {
            if (_timeoutToken != null)
                _timeoutToken.Dispose();
            _timeoutToken = _scheduler.Schedule(Timeout, () => _fsm.Fire(IsoTpEvent.Timeout));
        }

        private void DecockTimer()
        {
            if (_timeoutToken != null)
                _timeoutToken.Dispose();
        }

        private void AbortTransaction() { _port.Send(FlowControlFrame.AbortFrame); }

        private bool IsExpectedConsecutiveData(IsoTpFrame Frame)
        {
            var cf = (Frame as ConsecutiveFrame);
            if (cf == null)
                return false;
            return cf.Index == _receiveTransaction.ExpectedCounter;
        }

        private void WhenSingleFrameComes(SingleFrame Frame) { _fsm.Fire(IsoTpEvent.PackageReceived, new IsoTpPacket(Frame.Data)); }

        private void WhenFirstFrameComes(FirstFrame Frame)
        {
            _receiveTransaction = new ReceiveTransaction(Frame.PacketSize);
            _receiveTransaction.PushDataSlice(Frame.Data);
            SendFlowControl();
        }

        private void WhenConsecutiveDataComes(ConsecutiveFrame Frame)
        {
            _receiveTransaction.PushDataSlice(Frame.Data);

            if (_receiveTransaction.BlockCounter == 0)
            {
                SendFlowControl();
                _receiveTransaction.BlockCounter = _connectionParameters.BlockSize;
            }
            else
                _receiveTransaction.BlockCounter--;

            if (_receiveTransaction.Done)
                _fsm.Fire(IsoTpEvent.PackageReceived, _receiveTransaction.GetPacket());
        }

        private void SendFlowControl()
        {
            _port.Send(new FlowControlFrame(FlowControlFlag.ClearToSend, (byte)_connectionParameters.BlockSize, _connectionParameters.SeparationTime));
        }

        private void BeginTransmitTransaction(IsoTpPacket Packet)
        {
            _transmitTransaction = new TransmitTransaction(Packet.Data);
            byte[] payload = _transmitTransaction.GetDataSlice(FirstFrame.GetPayload(_port.Options.SublayerFrameCapacity));
            _port.Send(new FirstFrame(payload, Packet.Data.Length));
        }

        private void SendNextDataPortion(FlowControlFrame Frame)
        {
            for (int i = 0; i < Frame.BlockSize; i++)
            {
                byte[] payload = _transmitTransaction.GetDataSlice(ConsecutiveFrame.GetPayload(_port.Options.SublayerFrameCapacity));
                _port.Send(new ConsecutiveFrame(payload, _transmitTransaction.Index));
                _transmitTransaction.IncreaseIndex();
                if (_transmitTransaction.Done)
                {
                    _fsm.Fire(IsoTpEvent.PackageSent);
                    return;
                }
            }
        }

        #region IPort Members

        /// <summary>Поток входящих сообщений</summary>
        public IObservable<IsoTpPacket> Rx
        {
            get { return _rx; }
        }

        /// <summary>Поток исходящих сообщений</summary>
        public IObserver<IsoTpPacket> Tx { get; private set; }

        /// <summary>Опции порта</summary>
        public DataPortOptions<IsoTpPacket> Options { get; private set; }

        #endregion
    }
}
