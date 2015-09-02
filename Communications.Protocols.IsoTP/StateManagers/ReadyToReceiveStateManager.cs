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
        }
    }
}
