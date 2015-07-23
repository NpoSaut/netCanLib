using System;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Syntax;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.Transactions;

namespace Communications.Protocols.IsoTP.StateManagers
{
    internal class ReceiveStateManager : IStateManager
    {
        private readonly IsoTpConnectionParameters _connectionParameters;
        private readonly Action<IsoTpPacket> _emit;
        private readonly Action<Exception> _throw;
        private readonly ISender _sender;
        private readonly IStateMachine<IsoTpState, IsoTpEvent> _stateMachine;
        private readonly TimerManager _timerManager;
        private ReceiveTransaction _receiveTransaction;

        public ReceiveStateManager(IStateMachine<IsoTpState, IsoTpEvent> StateMachine, TimerManager TimerManager, ISender Sender,
                                   IsoTpConnectionParameters ConnectionParameters, Action<IsoTpPacket> EmitAction, Action<Exception> ThrowAction)
        {
            _stateMachine = StateMachine;
            _timerManager = TimerManager;
            _sender = Sender;
            _connectionParameters = ConnectionParameters;
            _emit = EmitAction;
            _throw = ThrowAction;

            StateMachine.In(IsoTpState.ReadyToReceive)
                        .On(IsoTpEvent.FrameReceived)
                        .If<IsoTpFrame>(f => f is SingleFrame)
                        .Goto(IsoTpState.ReadyToReceive)
                        .Execute<SingleFrame>(WhenSingleFrameComes);

            StateMachine.In(IsoTpState.ReadyToReceive)
                        .On(IsoTpEvent.FrameReceived)
                        .If<IsoTpFrame>(f => f is FirstFrame)
                        .Goto(IsoTpState.Receiving)
                        .Execute<FirstFrame>(WhenFirstFrameComes)
                        .Execute(() => _timerManager.CockTimer(_connectionParameters.ConsecutiveTimeout));

            StateMachine.In(IsoTpState.ReadyToReceive)
                        .On(IsoTpEvent.PackageReceived)
                        .Execute<IsoTpPacket>(p => _emit(p));

            IEntryActionSyntax<IsoTpState, IsoTpEvent> whenReceiving = StateMachine.In(IsoTpState.Receiving);

            whenReceiving
                .On(IsoTpEvent.FrameReceived)
                // Правильный пакет с данными
                .If<IsoTpFrame>(IsExpectedConsecutiveData)
                .Execute<ConsecutiveFrame>(WhenConsecutiveDataComes)
                .Execute(() => _timerManager.CockTimer(_connectionParameters.ConsecutiveTimeout))
                // Другой пакет с данными
                .If<IsoTpFrame>(f => f is ConsecutiveFrame)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction)
                .Execute<ConsecutiveFrame>(f => Throw(new IsoTpSequenceException(_receiveTransaction.ExpectedCounter, f)))
                // Новая короткая транзакция
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is SingleFrame)
                .Execute(() => Throw(new IsoTpTransactionLostException()))
                .Execute<SingleFrame>(WhenSingleFrameComes)
                // Новая длинная транзакция
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is FirstFrame)
                .Goto(IsoTpState.Receiving)
                .Execute(() => Throw(new IsoTpTransactionLostException()))
                .Execute<FirstFrame>(WhenFirstFrameComes)
                .Execute(() => _timerManager.CockTimer(_connectionParameters.ConsecutiveTimeout));

            whenReceiving
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is ConsecutiveFrame)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction)
                .Execute<IsoTpFrame>(f => Throw(new IsoTpWrongFrameException(f, typeof (ConsecutiveFrame))));

            whenReceiving
                .On(IsoTpEvent.Timeout)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction)
                .Execute(() => Throw(new IsoTpTimeoutException()));

            whenReceiving
                .On(IsoTpEvent.Abort)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction);

            whenReceiving
                .On(IsoTpEvent.PackageReceived)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute<IsoTpPacket>(p => _emit(p))
                .Execute(() => _receiveTransaction = null);

            whenReceiving
                .On(IsoTpEvent.TransmitRequest)
                .Execute(() => Throw(new IsoTpPortIsBusyException()));
        }

        private void Throw(Exception e) { _throw(e); }

        private void AbortTransaction() { _sender.Send(FlowControlFrame.AbortFrame); }

        private bool IsExpectedConsecutiveData(IsoTpFrame Frame)
        {
            var cf = (Frame as ConsecutiveFrame);
            if (cf == null)
                return false;
            return cf.Index == _receiveTransaction.ExpectedCounter;
        }

        private void WhenSingleFrameComes(SingleFrame Frame) { _stateMachine.Fire(IsoTpEvent.PackageReceived, new IsoTpPacket(Frame.Data)); }

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
                _stateMachine.Fire(IsoTpEvent.PackageReceived, _receiveTransaction.GetPacket());
        }

        private void SendFlowControl()
        {
            _sender.Send(new FlowControlFrame(FlowControlFlag.ClearToSend, (byte)_connectionParameters.BlockSize, _connectionParameters.SeparationTime));
        }
    }
}
