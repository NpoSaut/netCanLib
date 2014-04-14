using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Communications.Can;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;

namespace Communications.Protocols.IsoTP
{
    public interface IIsoTpConnection
    {
        int BlockSize { get; }

        void OnTransactionReady(TpReceiveTransaction Transaction);
        void SetNextState(IsoTpState NewState);
        void SendControlFrame();
    }

    public class CanIsoTpConnection : IIsoTpConnection
    {
        public CanIsoTpConnection(CanFlow Flow, ushort TransmitDescriptor, ushort ReceiveDescriptor, int BlockSize = 128)
        {
            this.BlockSize = BlockSize;
            this.ReceiveDescriptor = ReceiveDescriptor;
            this.TransmitDescriptor = TransmitDescriptor;
            this.Flow = Flow;
        }

        private CanFlow Flow { get; set; }
        private ushort TransmitDescriptor { get; set; }
        private ushort ReceiveDescriptor { get; set; }
        public int BlockSize { get; private set; }

        private IsoTpState ConnectionState { get; set; }

        private TpTransaction _readyTransaction;

        public Byte[] Receive(TimeSpan Timeout)
        {
            SetNextState(new ReadyToReceiveState(this));
            do
            {
                var frame = Flow.Read(Timeout)
                                .Where(f => f.Descriptor == ReceiveDescriptor)
                                .Select(f => IsoTpFrame.ParsePacket(f.Data))
                                .First();
                ConnectionState.ProcessFrame(frame);
            } while (_readyTransaction is TpReceiveTransaction);

            // ReSharper disable once PossibleInvalidCastException
            var res = ((TpReceiveTransaction)_readyTransaction).Data;
            _readyTransaction = null;
            return res;
        }

        public void OnTransactionReady(TpReceiveTransaction Transaction) { _readyTransaction = Transaction; }
        public void SetNextState(IsoTpState NewState) { ConnectionState = NewState; }
        public void SendControlFrame() { throw new NotImplementedException(); }
    }
}
