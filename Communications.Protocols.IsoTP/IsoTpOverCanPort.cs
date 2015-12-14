using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine.Events;
using Communications.Appi.Timeouts;
using Communications.Can;
using Communications.Options;
using Communications.PortHelpers;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.StateManagers;
using Communications.Protocols.IsoTP.Transactions;
using Communications.Transactions;

namespace Communications.Protocols.IsoTP
{
    internal enum IsoTpState
    {
        ReadyToReceive,
        Receiving,
        Transmiting,
        Disposed
    }

    internal enum IsoTpEvent
    {
        TransmitRequest,
        Abort,
        Timeout,
        FrameReceived,
        TransactionCompleated,
        Dispose
    }

    public class IsoTpOverCanPort : IIsoTpConnection
    {
        private readonly IsoTpConnectionParameters _connectionParameters;
        private readonly IStateMachine<IsoTpState, IsoTpEvent> _fsm;
        private readonly string _name;
        private readonly IIsoTpFramesPort _port;
        private readonly Subject<ITransaction<IsoTpPacket>> _rx;
        private readonly IDisposable _rxFromBelowConnection;
        private readonly EventLoopScheduler _scheduler;
        private readonly IStateManager[] _stateManagers;
        private readonly SchedulerTimeoutManager<TimeoutReason> _timeoutManager;

        public IsoTpOverCanPort(ICanPort CanPort, ushort TransmitDescriptor, ushort ReceiveDescriptor, IsoTpConnectionParameters ConnectionParameters)
            : this(
                CanPort, TransmitDescriptor, ReceiveDescriptor, String.Format("ISO-TP R{0:X4}/T{1:X4}", TransmitDescriptor, ReceiveDescriptor),
                ConnectionParameters) { }

        public IsoTpOverCanPort(ICanPort CanPort, ushort TransmitDescriptor, ushort ReceiveDescriptor, String Name,
                                IsoTpConnectionParameters ConnectionParameters)
        {
            Options = new IsoTpOverCanPortOptions(TransmitDescriptor, ReceiveDescriptor);
            _name = Name;
            _connectionParameters = ConnectionParameters;
            _scheduler = new EventLoopScheduler(ts => new Thread(ts) { Name = String.Format("{0} Thread", Name) });

            _port = new CanToIsoTpFramesPort(CanPort, TransmitDescriptor, ReceiveDescriptor);

            _rx = new Subject<ITransaction<IsoTpPacket>>();

            _fsm = new PassiveStateMachine<IsoTpState, IsoTpEvent>(Name);
            _timeoutManager = new SchedulerTimeoutManager<TimeoutReason>("ISO-TP", Reason => _fsm.Fire(IsoTpEvent.Timeout, Reason), _scheduler);
            var sender = new ActionSender(_port.BeginSend);
            _stateManagers = new IStateManager[]
                             {
                                 new ReadyToReceiveStateManager(_fsm, _timeoutManager),
                                 new ReceiveStateManager(_fsm, _timeoutManager, sender, _connectionParameters,
                                                         p => Task.Factory.StartNew(() => _rx.OnNext(p)),
                                                         e => Task.Factory.StartNew(() => _rx.OnError(e))),
                                 new SendStateManager(_fsm, _timeoutManager, _connectionParameters, sender, _port.Options.SublayerFrameCapacity)
                             };

            _fsm.In(IsoTpState.ReadyToReceive)
                .ExecuteOnEntry(_timeoutManager.DecockTimer);

            _fsm.In(IsoTpState.Disposed)
                .ExecuteOnEntry(OnDisposed);

            _fsm.Initialize(IsoTpState.ReadyToReceive);
            _fsm.TransitionExceptionThrown += FsmOnTransitionExceptionThrown;

            _rxFromBelowConnection =
                _port.Rx
                     .ObserveOn(_scheduler)
                     .WaitForTransactionCompleated()
                     .Subscribe(f => _fsm.Fire(IsoTpEvent.FrameReceived, f));

            _fsm.Start();
        }

        private void OnDisposed()
        {
            Debug.WriteLine("ON_DISPOSED {0}", this);
            _port.Dispose();
            _timeoutManager.Dispose();
            _scheduler.Dispose();
            _rxFromBelowConnection.Dispose();
            _fsm.Stop();
            _rx.Dispose();
        }

        public void Dispose()
        {
            Debug.WriteLine("DISPOSING {0}", this);
            _fsm.FirePriority(IsoTpEvent.Dispose);
            //_fsm.Stop();
            //_rx.OnCompleted();
        }

        private void FsmOnTransitionExceptionThrown(object Sender, TransitionExceptionEventArgs<IsoTpState, IsoTpEvent> e)
        {
            //_rx.OnError(e.Exception);
        }

        public override string ToString() { return String.Format("ISO-TP {{{0}}}", _name); }

        #region IPort Members

        /// <summary>Поток входящих сообщений</summary>
        public IObservable<ITransaction<IsoTpPacket>> Rx
        {
            get { return _rx; }
        }

        /// <summary>Поток исходящих сообщений</summary>
        public IObserver<IsoTpPacket> Tx { get; private set; }

        /// <summary>Опции порта</summary>
        public DataPortOptions<IsoTpPacket> Options { get; private set; }

        /// <summary>Начинает отправку кадра</summary>
        /// <param name="Frame">Кадр для отправки</param>
        /// <returns>Транзакция передачи</returns>
        public ITransaction<IsoTpPacket> BeginSend(IsoTpPacket Frame)
        {
            var transaction = new TransmitTransaction(Frame);
            _scheduler.Schedule(() => _fsm.Fire(IsoTpEvent.TransmitRequest, transaction));
            return transaction;
        }

        #endregion
    }
}
