using Appccelerate.StateMachine;

namespace Communications.Protocols.IsoTP.StateManagers
{
    internal class ReadyToReceiveStateManager : IStateManager
    {
        public ReadyToReceiveStateManager(IStateMachine<IsoTpState, IsoTpEvent> StateMachine, TimerManager TimerManager)
        {
            StateMachine.In(IsoTpState.ReadyToReceive)
                        .ExecuteOnEntry(TimerManager.DecockTimer);
        }
    }
}
