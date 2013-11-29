using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Communications
{
    public interface IPort<TDatagram> : ISocketSource<TDatagram>, ISendPipe<TDatagram>, IReceivePipe<TDatagram>, IDisposable
    {
        string Name { get; }
    }

    public interface ISocketSource<TDatagram> : IDisposable
    {
        ISocket<TDatagram> OpenSocket();
        event EventHandler AllSocketsDisposed;
    }

    public interface ISendPipe<TDatagram>
    {
        void Send(IList<TDatagram> Data);
    }

    public interface IReceivePipe<TDatagram>
    {
        void ObtainReceived(IList<TDatagram> Datagrams);
    }
}