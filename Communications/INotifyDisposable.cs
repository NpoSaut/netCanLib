using System;

namespace Communications
{
    public interface INotifyDisposable
    {
        event EventHandler Disposed;
    }
}
