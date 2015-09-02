namespace Communications.Protocols.IsoTP.StateManagers
{
    public enum TimeoutReason
    {
        WaitingForFirstFlowControl,
        WaitingForConsecutiveFrameAfterFirstFlowControl,
        WaitingForNextConsecutiveFrame,
        WaitingForFlowControlFrameAfterWaitFrame,
        WaitingForFlowControlFrameAfterDataPortionSent,
        WaitingForConsecutiveFrameAfterFirstFlowControlInInterruptingTransaction
    }
}
