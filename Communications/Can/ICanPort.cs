using System;

namespace Communications.Can
{
    public interface ICanPort
    {
        IObservable<CanFrame> Rx { get; }
    }
}