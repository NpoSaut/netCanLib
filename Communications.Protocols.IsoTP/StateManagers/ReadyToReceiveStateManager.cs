using Appccelerate.StateMachine;
using Communications.Appi.Timeouts;

namespace Communications.Protocols.IsoTP.StateManagers
{
    internal class ReadyToReceiveStateManager : IStateManager
    {
        public ReadyToReceiveStateManager(IStateMachine<IsoTpState, IsoTpEvent> StateMachine, ITimeoutManager<TimeoutReason> TimerManager)
        {
            StateMachine.In(IsoTpState.ReadyToReceive)
                        .ExecuteOnEntry(TimerManager.DecockTimer);

            StateMachine.In(IsoTpState.ReadyToReceive)
                        .On(IsoTpEvent.Dispose)
                        .Goto(IsoTpState.Disposed);
        }
    }
}
