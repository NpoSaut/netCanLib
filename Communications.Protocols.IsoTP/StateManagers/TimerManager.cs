using System;
using System.Reactive.Concurrency;
using Appccelerate.StateMachine;

namespace Communications.Protocols.IsoTP.StateManagers
{
    internal enum TimeoutReason
    {
        WaitingForFirstFlowControl,
        WaitingForConsecutiveFrameAfterFirstFlowControl,
        WaitingForNextConsecutiveFrame,
        WaitingForFlowControlFrameAfterWaitFrame,
        WaitingForFlowControlFrameAfterDataPortionSent,
        WaitingForConsecutiveFrameAfterFirstFlowControlInInterruptingTransaction
    }

    internal class TimerManager
    {
        private readonly IScheduler _scheduler;
        private readonly IStateMachine<IsoTpState, IsoTpEvent> _stateMachine;
        private IDisposable _timeoutToken;

        public TimerManager(IStateMachine<IsoTpState, IsoTpEvent> StateMachine, IScheduler Scheduler)
        {
            _stateMachine = StateMachine;
            _scheduler = Scheduler;
        }

        public void CockTimer(TimeSpan Timeout, TimeoutReason Reason)
        {
            if (_timeoutToken != null)
                _timeoutToken.Dispose();
            _timeoutToken = _scheduler.Schedule(Timeout, () => _stateMachine.Fire(IsoTpEvent.Timeout, Reason));
        }

        public void DecockTimer()
        {
            if (_timeoutToken != null)
                _timeoutToken.Dispose();
        }
    }
}
