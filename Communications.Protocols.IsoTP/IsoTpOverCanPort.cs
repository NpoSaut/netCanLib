using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Machine.Events;
using Communications.Can;
using Communications.Protocols.IsoTP.StateManagers;
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
        private readonly EventLoopScheduler _scheduler;
        private IStateManager[] _stateManagers;

        public IsoTpOverCanPort(ICanPort CanPort, ushort TransmitDescriptor, ushort ReceiveDescriptor, IsoTpConnectionParameters ConnectionParameters)
        {
            Options = new IsoTpOverCanPortOptions(TransmitDescriptor, ReceiveDescriptor);
            _connectionParameters = ConnectionParameters;
            _scheduler = new EventLoopScheduler();

            _port = new CanToIsoTpFramesPort(CanPort, TransmitDescriptor, ReceiveDescriptor);

            _rx = new Subject<IsoTpPacket>();

            _fsm = new PassiveStateMachine<IsoTpState, IsoTpEvent>();
            var timeManager = new TimerManager(_fsm, _scheduler);
            var sender = new ActionSender(_port.Tx.OnNext);
            _stateManagers = new IStateManager[]
                             {
                                 new ReadyToReceiveStateManager(_fsm, timeManager),
                                 new ReceiveStateManager(_fsm, timeManager, sender, _connectionParameters,
                                                         p => Task.Factory.StartNew(() => _rx.OnNext(p)),
                                                         e => Task.Factory.StartNew(() => _rx.OnError(e))),
                                 new SendStateManager(_fsm, timeManager, _connectionParameters, sender, _port.Options.SublayerFrameCapacity)
                             };

            _fsm.In(IsoTpState.ReadyToReceive)
                .ExecuteOnEntry(timeManager.DecockTimer);

            _fsm.Initialize(IsoTpState.ReadyToReceive);
            _fsm.TransitionExceptionThrown += FsmOnTransitionExceptionThrown;

            _rxFromBelowConnection =
                _port.Rx
                     .SubscribeOn(_scheduler)
                     .Subscribe(f => _fsm.Fire(IsoTpEvent.FrameReceived, f));

            Tx = Observer.Create<IsoTpPacket>(TransmitPacket,
                                              e => _fsm.Fire(IsoTpEvent.Abort));

            _fsm.Start();
        }

        public void Dispose()
        {
            _fsm.Stop();
            _rxFromBelowConnection.Dispose();
            _rx.Dispose();
            _port.Dispose();
            _scheduler.Dispose();
        }

        private void TransmitPacket(IsoTpPacket p)
        {
            var transaction = new TransmitTransaction(p.Data);
            _scheduler.Schedule(() => _fsm.Fire(IsoTpEvent.TransmitRequest, transaction));
            transaction.Wait();
        }

        private void FsmOnTransitionExceptionThrown(object Sender, TransitionExceptionEventArgs<IsoTpState, IsoTpEvent> e)
        {
            //_rx.OnError(e.Exception);
        }

        public override string ToString()
        {
            return String.Format("ISO-TP [R{0:X4}/T{1:X4}] Port", _port.Options.ReceiveDescriptor, _port.Options.TransmitDescriptor);
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
