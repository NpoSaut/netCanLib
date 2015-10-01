using System;
using System.Diagnostics;
using System.Threading;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Syntax;
using Communications.Appi.Timeouts;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.Transactions;
using Communications.Transactions;
using NLog;

namespace Communications.Protocols.IsoTP.StateManagers
{
    internal class ReceiveStateManager : IStateManager
    {
        private readonly IsoTpConnectionParameters _connectionParameters;
        private readonly Action<ITransaction<IsoTpPacket>> _emit;
        private readonly ISender _sender;
        private readonly IStateMachine<IsoTpState, IsoTpEvent> _stateMachine;
        private readonly Action<Exception> _throw;
        private readonly ITimeoutManager<TimeoutReason> _timeoutManager;
        private ReceiveTransaction _receiveTransaction;

        private readonly ILogger _logger = LogManager.GetLogger("ISO-TP");

        public ReceiveStateManager(IStateMachine<IsoTpState, IsoTpEvent> StateMachine, ITimeoutManager<TimeoutReason> TimeoutManager, ISender Sender,
                                   IsoTpConnectionParameters ConnectionParameters,
                                   Action<ITransaction<IsoTpPacket>> EmitAction, Action<Exception> ThrowAction)
        {
            _stateMachine = StateMachine;
            _timeoutManager = TimeoutManager;
            _sender = Sender;
            _connectionParameters = ConnectionParameters;
            _emit = EmitAction;
            _throw = ThrowAction;

            StateMachine.In(IsoTpState.ReadyToReceive)
                        .On(IsoTpEvent.FrameReceived)
                        .If<IsoTpFrame>(f => f is SingleFrame)
                        .Execute<SingleFrame>(WhenSingleFrameComes);

            StateMachine.In(IsoTpState.ReadyToReceive)
                        .On(IsoTpEvent.FrameReceived)
                        .If<IsoTpFrame>(f => f is FirstFrame)
                        .Goto(IsoTpState.Receiving)
                        .Execute<FirstFrame>(WhenFirstFrameComes)
                        .Execute(() => _timeoutManager.CockTimer(_connectionParameters.ConsecutiveTimeout,
                                                               TimeoutReason.WaitingForConsecutiveFrameAfterFirstFlowControl));

            IEntryActionSyntax<IsoTpState, IsoTpEvent> whenReceiving = StateMachine.In(IsoTpState.Receiving);

            whenReceiving
                .On(IsoTpEvent.FrameReceived)
                    // Правильный пакет с данными
                    .If<IsoTpFrame>(IsExpectedConsecutiveData)
                        .Execute<ConsecutiveFrame>(WhenConsecutiveDataComes)
                        .Execute(() => _timeoutManager.CockTimer(_connectionParameters.ConsecutiveTimeout, TimeoutReason.WaitingForNextConsecutiveFrame))
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
                .Execute(() => _timeoutManager.CockTimer(_connectionParameters.ConsecutiveTimeout,
                                                       TimeoutReason.WaitingForConsecutiveFrameAfterFirstFlowControlInInterruptingTransaction));

            whenReceiving
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is ConsecutiveFrame)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction)
                .Execute<IsoTpFrame>(f => Throw(new IsoTpWrongFrameException(f, typeof (ConsecutiveFrame))));

            whenReceiving
                .On(IsoTpEvent.Timeout)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(() => _logger.Error("Таймаут вышел. Последний принятый индекс: {0}", _receiveTransaction.ExpectedCounter - 1))
                .Execute(AbortTransaction)
                .Execute<TimeoutReason>(r => Throw(new IsoTpTimeoutException(r)));

            whenReceiving
                .On(IsoTpEvent.Abort)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(AbortTransaction);

            whenReceiving
                .On(IsoTpEvent.TransactionCompleated)
                    .Goto(IsoTpState.ReadyToReceive)
                    .Execute(() => _logger.Debug("Commit! THREAD: {0}", Thread.CurrentThread.Name))
                    .Execute(() => _receiveTransaction.Commit())
                    .Execute(() => _receiveTransaction = null);

            whenReceiving
                .On(IsoTpEvent.TransmitRequest)
                .Execute(() => Throw(new IsoTpPortIsBusyException()));
        }

        private void AbortTransaction() { _sender.Send(FlowControlFrame.AbortFrame); }

        private bool IsExpectedConsecutiveData(IsoTpFrame Frame)
        {
            var cf = (Frame as ConsecutiveFrame);
            if (cf == null)
                return false;
            return cf.Index == _receiveTransaction.ExpectedCounter;
        }

        private void WhenSingleFrameComes(SingleFrame Frame)
        {
            _emit(new InstantaneousTransaction<IsoTpPacket>(new IsoTpPacket(Frame.Data)));
        }

        private void WhenFirstFrameComes(FirstFrame Frame)
        {
            _receiveTransaction = new ReceiveTransaction(Frame.PacketSize, _connectionParameters.BlockSize);
            _receiveTransaction.PushDataSlice(Frame.Data);
            _emit(_receiveTransaction);
            SendFlowControl();
        }

        private void WhenConsecutiveDataComes(ConsecutiveFrame Frame)
        {
            _receiveTransaction.PushDataSlice(Frame.Data);

            _receiveTransaction.BlockCounter--;
            if (_receiveTransaction.BlockCounter == 0)
            {
                SendFlowControl();
                _receiveTransaction.BlockCounter = _connectionParameters.BlockSize;
            }

            if (_receiveTransaction.AllDataReceived)
                _stateMachine.Fire(IsoTpEvent.TransactionCompleated);
        }

        private void Throw(Exception e) { _receiveTransaction.Fail(e); }

        private void SendFlowControl()
        {
            _sender.Send(new FlowControlFrame(FlowControlFlag.ClearToSend, (byte)_connectionParameters.BlockSize, _connectionParameters.SeparationTime));
        }
    }
}
