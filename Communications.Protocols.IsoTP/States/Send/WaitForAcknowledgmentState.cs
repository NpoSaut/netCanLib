using System;
using System.Reactive.Linq;
using Communications.Protocols.IsoTP.Contexts;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States.Send
{
    public class WaitForAcknowledgmentState : IsoTpStateBase
    {
        private readonly IsoTpTransmitTransactionContext _context;
        public WaitForAcknowledgmentState(IsoTpTransmitTransactionContext Context) { _context = Context; }

        public override IIsoTpState Operate(IsoTpFrame Frame)
        {
            switch (Frame.FrameType)
            {
                case IsoTpFrameType.FlowControl:
                    var cf = (FlowControlFrame)Frame;
                    switch (cf.Flag)
                    {
                        case FlowControlFlag.ClearToSend:
                            return SendNextBlock(cf.BlockSize, cf.SeparationTime)
                                       ? (IIsoTpState)new IdleSendState()
                                       : new WaitForAcknowledgmentState(_context);

                        case FlowControlFlag.Wait:
                            break;

                        case FlowControlFlag.Abort:
                            _context.IsAborted = true;
                            throw new IsoTpTransactionAbortedException();
                    }
                    break;

                case IsoTpFrameType.First:
                case IsoTpFrameType.Single:
                case IsoTpFrameType.Consecutive:
                    throw new IsoTpWrongFrameException(Frame, typeof (ConsecutiveFrame));
            }
            return this;
        }

        private bool SendNextBlock(byte BlockSize, TimeSpan SeparationTime)
        {
            int payload = ConsecutiveFrame.GetPayload(_context.FrameLayerCapacity);
            Observable.Range(0, BlockSize)
                      .TakeWhile(i => !_context.IsReady && !_context.IsAborted)
                      .Delay(SeparationTime)
                      .Select(i => new ConsecutiveFrame(_context.Read(payload), _context.Index++))
                      .Subscribe(_context.Tx);
            return _context.IsReady;
        }
    }
}
