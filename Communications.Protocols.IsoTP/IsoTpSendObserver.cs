using System;
using System.Reactive;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.Transactions;

namespace Communications.Protocols.IsoTP
{
    public class IsoTpSendObserver : ObserverBase<IsoTpPacket>
    {
        private readonly int _frameLayerCapacity;
        private readonly IObservable<IsoTpFrame> _rx;
        private readonly TimeSpan _timeout;
        private readonly IObserver<IsoTpFrame> _tx;

        public IsoTpSendObserver(IObservable<IsoTpFrame> Rx, IObserver<IsoTpFrame> Tx, TimeSpan Timeout, int FrameLayerCapacity)
        {
            _rx = Rx;
            _tx = Tx;
            _timeout = Timeout;
            _frameLayerCapacity = FrameLayerCapacity;
        }

        /// <summary>Implement this method to react to the receival of a new element in the sequence.</summary>
        /// <param name="Packet">Next element in the sequence.</param>
        /// <remarks>This method only gets called when the observer hasn't stopped yet.</remarks>
        protected override void OnNextCore(IsoTpPacket Packet) { new IsoTpTransmitTransaction(_rx, _tx, _frameLayerCapacity).Send(Packet, _timeout); }

        /// <summary>Implement this method to react to the occurrence of an exception.</summary>
        /// <param name="error">The error that has occurred.</param>
        /// <remarks>This method only gets called when the observer hasn't stopped yet, and causes the observer to stop.</remarks>
        protected override void OnErrorCore(Exception error) { }

        /// <summary>Implement this method to react to the end of the sequence.</summary>
        /// <remarks>This method only gets called when the observer hasn't stopped yet, and causes the observer to stop.</remarks>
        protected override void OnCompletedCore() { }
    }
}
