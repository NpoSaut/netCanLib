using System;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Syntax;
using Communications.Appi.Timeouts;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.Transactions;

namespace Communications.Protocols.IsoTP.StateManagers
{
    internal class SendStateManager : IStateManager
    {
        private readonly IsoTpConnectionParameters _connectionParameters;
        private readonly ISender _sender;
        private readonly IStateMachine<IsoTpState, IsoTpEvent> _stateMachine;
        private readonly int _sublayerFrameCapacity;
        private readonly ITimeoutManager<TimeoutReason> _timeoutManager;

        private TransmitTransaction _transmitTransaction;

        public SendStateManager(IStateMachine<IsoTpState, IsoTpEvent> StateMachine, ITimeoutManager<TimeoutReason> TimeoutManager,
                                IsoTpConnectionParameters ConnectionParameters,
                                ISender Sender, int SublayerFrameCapacity)
        {
            _stateMachine = StateMachine;
            _timeoutManager = TimeoutManager;
            _connectionParameters = ConnectionParameters;
            _sender = Sender;
            _sublayerFrameCapacity = SublayerFrameCapacity;

            StateMachine.In(IsoTpState.ReadyToReceive)
                        .On(IsoTpEvent.TransmitRequest)
                // Маленький пакет
                        .If<TransmitTransaction>(t => t.Length <= SingleFrame.GetPayload(_sublayerFrameCapacity))
                        .Execute<TransmitTransaction>(SendShortTransaction)
                // Большой пакет
                        .Otherwise()
                        .Goto(IsoTpState.Transmiting)
                        .Execute<TransmitTransaction>(BeginTransmitTransaction)
                        .Execute(() => _timeoutManager.CockTimer(_connectionParameters.FirstResponseTimeout, TimeoutReason.WaitingForFirstFlowControl));

            IEntryActionSyntax<IsoTpState, IsoTpEvent> whenTransmiting = StateMachine.In(IsoTpState.Transmiting);

            whenTransmiting
                .On(IsoTpEvent.FrameReceived)
                .If<IsoTpFrame>(f => f is FlowControlFrame && ((FlowControlFrame)f).Flag == FlowControlFlag.ClearToSend)
                .Execute(() => _timeoutManager.DecockTimer())
                .Execute<FlowControlFrame>(SendNextDataPortion)
                //.Execute(() => _timeoutManager.CockTimer(_connectionParameters.ConsecutiveTimeout, TimeoutReason.WaitingForFlowControlFrameAfterDataPortionSent))
                .If<IsoTpFrame>(f => f is FlowControlFrame && ((FlowControlFrame)f).Flag == FlowControlFlag.Wait)
                .Execute(() => _timeoutManager.CockTimer(_connectionParameters.ConsecutiveTimeout, TimeoutReason.WaitingForFlowControlFrameAfterWaitFrame))
                .If<IsoTpFrame>(f => f is FlowControlFrame && ((FlowControlFrame)f).Flag == FlowControlFlag.Abort)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(() => Throw(new IsoTpTransactionAbortedException()))
                .Otherwise()
                .Execute<IsoTpFrame>(f => Throw(new IsoTpWrongFrameException(f, typeof (FlowControlFrame))));

            whenTransmiting
                .On(IsoTpEvent.Timeout)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute<TimeoutReason>(r => Throw(new IsoTpTimeoutException(r)));

            whenTransmiting
                .On(IsoTpEvent.Abort)
                .Goto(IsoTpState.ReadyToReceive);

            whenTransmiting
                .On(IsoTpEvent.TransmitRequest)
                .Execute(() => Throw(new IsoTpPortIsBusyException()));

            whenTransmiting
                .On(IsoTpEvent.TransactionCompleated)
                .Goto(IsoTpState.ReadyToReceive)
                .Execute(() => _transmitTransaction.Commit());
        }

        private void Throw(Exception e) { _transmitTransaction.Fail(e); }

        private void SendShortTransaction(TransmitTransaction Transaction)
        {
            try
            {
                _sender.Send(new SingleFrame(Transaction.GetDataSlice(Transaction.Length)));
                Transaction.Commit();
            }
            catch (Exception e)
            {
                Transaction.Fail(e);
            }
        }

        private void BeginTransmitTransaction(TransmitTransaction Transaction)
        {
            _transmitTransaction = Transaction;
            byte[] payload = _transmitTransaction.GetDataSlice(FirstFrame.GetPayload(_sublayerFrameCapacity));
            _sender.Send(new FirstFrame(payload, (int)Transaction.Length));
        }

        private void SendNextDataPortion(FlowControlFrame Frame)
        {
            //IList<ITransaction<IsoTpFrame>> transactions = new List<ITransaction<IsoTpFrame>>();
            for (int i = 0; i < Frame.BlockSize; i++)
            {
                byte[] payload = _transmitTransaction.GetDataSlice(ConsecutiveFrame.GetPayload(_sublayerFrameCapacity));
                //transactions.Add(
                _sender.Send(new ConsecutiveFrame(payload, _transmitTransaction.Index)).Wait();
                _transmitTransaction.IncreaseIndex();
                if (_transmitTransaction.AllDataSent)
                {
                    _stateMachine.Fire(IsoTpEvent.TransactionCompleated);
                    break;
                }
            }
            //foreach (var transaction in transactions)
            //    transaction.Wait();
            _timeoutManager.CockTimer(_connectionParameters.ConsecutiveTimeout, TimeoutReason.WaitingForFlowControlFrameAfterDataPortionSent);
        }
    }
}
