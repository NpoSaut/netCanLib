using System;

namespace Communications.Can
{
    public interface ICanPort
    {
        IObservable<CanFrame> Rx { get; }
        IObserver<CanFrame> Tx { get; }
    }
}