using System;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.States;

namespace Communications.Protocols.IsoTP
{
    public interface IIsoTpConnection
    {
        int ReceiveBlockSize { get; }
        TimeSpan ReceiveSeparationTime { get; }
        int MaximumDatagramLength { get; }
        int SubframeLength { get; }

        void OnTransactionReady(TpTransaction Transaction);
        void SetNextState(IsoTpStateBase NewState);

        IsoTpFrame ReadNextFrame(TimeSpan Timeout);
        void SendFrame(IsoTpFrame Frame);

        Byte[] Receive(TimeSpan Timeout);
    }
}
