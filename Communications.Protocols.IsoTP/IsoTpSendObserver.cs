using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Communications.Protocols.IsoTP.Contexts;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;
using Communications.Protocols.IsoTP.States.Send;

namespace Communications.Protocols.IsoTP
{
    public class IsoTpSendObserver : ObserverBase<IsoTpPacket>
    {
        private readonly int _frameLayerCapacity;
        private readonly IDisposable _rxConnection;
        private readonly EventLoopScheduler _scheduler = new EventLoopScheduler();
        private readonly IObserver<IsoTpFrame> _tx;
        private IIsoTpState _currentState;

        /// <summary>Creates a new observer in a non-stopped state.</summary>
        public IsoTpSendObserver(IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx, int FrameLayerCapacity)
        {
            _tx = Tx;
            _frameLayerCapacity = FrameLayerCapacity;
            _currentState = new IdleSendState();

            _rxConnection = Rx.ObserveOn(_scheduler)
                              .Subscribe(f => _currentState.Operate(f));
        }

        /// <summary>Implement this method to react to the receival of a new element in the sequence.</summary>
        /// <param name="Packet">Next element in the sequence.</param>
        /// <remarks>This method only gets called when the observer hasn't stopped yet.</remarks>
        protected override void OnNextCore(IsoTpPacket Packet)
        {
            var context = new IsoTpTransmitTransactionContext(Packet, _frameLayerCapacity, _tx);

            if (Packet.Data.Length <= SingleFrame.GetPayload(_frameLayerCapacity))
                _scheduler.Schedule(() => SendSingleFrame(context));
            else
            {
                _scheduler.Schedule(() => SendFirstFrame(context));
                _currentState = new WaitForAcknowledgmentState(context);
            }

            SpinWait.SpinUntil(() => context.IsReady);
        }

        private void SendFirstFrame(IsoTpTransmitTransactionContext Context)
        {
            var frame = new FirstFrame(Context.Read(FirstFrame.GetPayload(_frameLayerCapacity)), Context.PacketLength);
            _tx.OnNext(frame);
        }

        private void SendSingleFrame(IsoTpTransmitTransactionContext Context)
        {
            var frame = new SingleFrame(Context.Read(Context.PacketLength));
            _tx.OnNext(frame);
            Context.Submit();
        }

        /// <summary>Implement this method to react to the occurrence of an exception.</summary>
        /// <param name="error">The error that has occurred.</param>
        /// <remarks>This method only gets called when the observer hasn't stopped yet, and causes the observer to stop.</remarks>
        protected override void OnErrorCore(Exception error) { throw new NotImplementedException(); }

        /// <summary>Implement this method to react to the end of the sequence.</summary>
        /// <remarks>This method only gets called when the observer hasn't stopped yet, and causes the observer to stop.</remarks>
        protected override void OnCompletedCore() { }

        /// <summary>Core implementation of IDisposable.</summary>
        /// <param name="disposing">
        ///     true if the Dispose call was triggered by the IDisposable.Dispose method; false if it was
        ///     triggered by the finalizer.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            _rxConnection.Dispose();
            _scheduler.Dispose();
            base.Dispose(disposing);
        }
    }
}
