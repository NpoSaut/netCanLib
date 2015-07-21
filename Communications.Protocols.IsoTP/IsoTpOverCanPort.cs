using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Can;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.ReceiveStates;
using Communications.Protocols.IsoTP.Transactions;

namespace Communications.Protocols.IsoTP
{
    internal enum IsoTpState
    {
        ReadyToReceive,
        WaitForControlField,
        WaitingForFlowControl
    }

    public class IsoTpOverCanPort : IIsoTpConnection
    {
        private readonly IsoTpConnectionParameters _connectionParameters;
        private readonly IIsoTpFramesPort _port;
        private readonly Subject<IsoTpPacket> _rx;
        private readonly IDisposable _rxFromBelowConnection;
        private readonly IScheduler _scheduler;

        private IIsoTpState _currentState;

        private IsoTpState _state = IsoTpState.ReadyToReceive;

        public IsoTpOverCanPort(ICanPort CanPort, ushort TransmitDescriptor, ushort ReceiveDescriptor, IsoTpConnectionParameters ConnectionParameters)
        {
            _connectionParameters = ConnectionParameters;
            _scheduler = new EventLoopScheduler();

            _port = new CanToIsoTpFramesPort(CanPort, TransmitDescriptor, ReceiveDescriptor);

            _rxFromBelowConnection =
                _port.Rx
                     .SubscribeOn(_scheduler)
                     .Subscribe(OnReceiveFromBelow, OnErrorFromBelow);

            _currentState = CreateReadyToReceiveState();

            Tx = Observer.Create<IsoTpPacket>(Transmit, OnExceptionFromAbove);

            _rx = new Subject<IsoTpPacket>();
            Rx = _rx;
        }

        public void Dispose()
        {
            _rxFromBelowConnection.Dispose();
            _rx.Dispose();
            _port.Dispose();
        }

        private void SetState(IsoTpState NewState)
        {
            if (_state == NewState)
                return;

            IsoTpState old = _state;
            _state = NewState;
            OnStateTransition(old, NewState);
        }

        private void OnStateTransition(IsoTpState OldState, IsoTpState NewState)
        {
            foreach (StateTransitionAction transition in
                _transitions.Where(t => t.OldState == OldState && t.NewState == NewState))
                transition.Activate();
        }

        private void OnReceiveFromBelow(IsoTpFrame Frame) { SetState(_currentState.Operate(Frame)); }

        private void OnErrorFromBelow(Exception e) { _currentState.OnException(e); }

        private void SetState(IIsoTpState NewState)
        {
            if (_currentState == NewState)
                return;
            _currentState.Dispose();
            _currentState = NewState;
            _scheduler.Schedule(_currentState.Activate);
        }

        private void Transmit(IsoTpPacket Packet)
        {
            if (!(_currentState is ReadyToReceiveState))
                throw new IsoTpPortIsBusyException();

            if (Packet.Data.Length <= _port.Options.SublayerFrameCapacity)
                _port.Send(new SingleFrame(Packet.Data));

            SetState(new TransmitState(Packet, _port, _connectionParameters));
        }

        private void OnExceptionFromAbove(Exception e)
        {
            _currentState.OnException(e);
            if (_currentState is ReadyToReceiveState)
                return;
            SetState(CreateReadyToReceiveState());
            _port.Tx.OnError(e);
        }

        private IIsoTpState CreateReadyToReceiveState() { return new ReadyToReceiveState(_rx, _port.Tx, _connectionParameters); }

        #region IPort Members

        /// <summary>Поток входящих сообщений</summary>
        public IObservable<IsoTpPacket> Rx { get; private set; }

        /// <summary>Поток исходящих сообщений</summary>
        public IObserver<IsoTpPacket> Tx { get; private set; }

        /// <summary>Опции порта</summary>
        public DataPortOptions<IsoTpPacket> Options { get; private set; }

        #endregion

        #region Состояния

        private readonly IDictionary<IsoTpState, StateReaction> _stateReactions;

        private BinaryReader _packetBodyReader;
        private IDisposable _timeout;

        public IsoTpOverCanPort()
        {
            _stateReactions =
                new Dictionary<IsoTpState, StateReaction>
                {
                    {
                        IsoTpState.ReadyToReceive,
                        new StateReaction
                        {
                            TransmitReactions = new TransmitReactionStrategy
                                                {
                                                    OnSmallPacket = new StateTransition<Action<IsoTpPacket>>(IsoTpState.ReadyToReceive,
                                                                                                             p => SendFrames(new SingleFrame(p.Data))),
                                                    OnBigPacket = new StateTransition<Action<IsoTpPacket>>(IsoTpState.WaitingForFlowControl,
                                                                                                           BeginTransaction)
                                                },
                            OnAbort = new StateTransition<Action>(IsoTpState.ReadyToReceive)
                        }
                    }
                };
        }

        #region Воздействия

        private void OnTransmit(IsoTpPacket Packet)
        {
            TransmitReactionStrategy transmitReactions = _stateReactions[_state].TransmitReactions;
            StateTransition<Action<IsoTpPacket>> transition = Packet.Data.Length <= SingleFrame.GetPayload(_port.Options.SublayerFrameCapacity)
                                                                  ? transmitReactions.OnSmallPacket
                                                                  : transmitReactions.OnBigPacket;
            ExecuteTransmition(transition, a => a(Packet));
        }

        private void OnTimeout() { ExecuteTransmition(_stateReactions[_state].OnTimeout, a => a()); }

        private void ExecuteTransmition<TAction>(StateTransition<TAction> transition, Action<TAction> ActionExecution)
        {
            transition.TransitionAction.ForEach(ActionExecution);
            _state = transition.NewState;
        }

        #endregion

        private void BeginTransaction(IsoTpPacket Packet)
        {
            _packetBodyReader = new BinaryReader(new MemoryStream(Packet.Data));
            SendFrames(new FirstFrame(_packetBodyReader.ReadBytes(FirstFrame.GetPayload(_port.Options.SublayerFrameCapacity)), Packet.Data.Length));
            _timeout = _scheduler.Schedule(_connectionParameters.FirstResponseTimeout, OnTimeout);
        }

        private void SendFrames(params IsoTpFrame[] Frames)
        {
            foreach (IsoTpFrame frame in Frames)
                _port.Tx.OnNext(frame);
        }

        private class FrameProcessingStrategy
        {
            public FrameProcessingStrategy()
            {
                OnSingleFrame = Frame => { };
                OnFirstFrame = Frame => { };
                OnConsecutiveFrame = Frame => { };
                OnFlowControlFrame = Frame => { };
            }

            public StateTransition<Action<SingleFrame>> OnSingleFrame { get; set; }
            public StateTransition<Action<FirstFrame>> OnFirstFrame { get; set; }
            public StateTransition<Action<ConsecutiveFrame>> OnConsecutiveFrame { get; set; }
            public StateTransition<Action<FlowControlFrame>> OnFlowControlFrame { get; set; }
        }

        private class StateReaction
        {
            public FrameProcessingStrategy FrameProcessing { get; set; }
            public StateTransition<Action> OnTimeout { get; set; }
            public StateTransition<Action> OnAbort { get; set; }
            public TransmitReactionStrategy TransmitReactions { get; set; }
        }

        private class StateTransition<TAction>
        {
            public StateTransition(IsoTpState NewState, params TAction[] TransitionAction)
            {
                this.NewState = NewState;
                this.TransitionAction = TransitionAction;
            }

            public IsoTpState NewState { get; private set; }
            public IList<TAction> TransitionAction { get; private set; }
        }

        private class TransmitReactionStrategy
        {
            public StateTransition<Action<IsoTpPacket>> OnBigPacket { get; set; }
            public StateTransition<Action<IsoTpPacket>> OnSmallPacket { get; set; }
        }

        #endregion
    }
}
