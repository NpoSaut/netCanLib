using System;
using Communications.Appi.Exceptions;
using Communications.Appi.Ports;
using Communications.Can;
using Communications.Transactions;

namespace Communications.Appi
{
    public class AppiCanTransmitTransaction : LongTransactionBase<CanFrame>
    {
        private readonly CanFrame _frame;
        private readonly AppiCanPort _port;

        public AppiCanTransmitTransaction(CanFrame Frame, AppiCanPort Port)
        {
            _frame = Frame;
            _port = Port;
            _port.Disposed += PortOnDisposed;
        }

        protected override CanFrame GetPayload() { return _frame; }

        protected override void OnCompleated()
        {
            _port.Disposed -= PortOnDisposed;
            base.OnCompleated();
        }

        private void PortOnDisposed(object Sender, EventArgs EventArgs) { Fail(new AppiPortClosedException()); }
    }
}
