using System;
using Communications.Protocols.IsoTP.Contexts;
using Communications.Protocols.IsoTP.Frames;

namespace Communications.Protocols.IsoTP.States.Send
{
//    public class BeginTransmitionState : IsoTpStateBase
//    {
//        private readonly IsoTpTransmitTransactionContext _context;
//        public BeginTransmitionState(IsoTpTransmitTransactionContext Context) { _context = Context; }
//
//        public void SomeFunction()
//        {
//            if (_context.PacketLength <= SingleFrame.GetPayload(_context.FrameLayerCapacity))
//            {
//                var frame = new SingleFrame(_context.Read(_context.PacketLength));
//                _context.Tx.OnNext(frame);
//                _context.Submit();
//            }
//            else
//            {
//                var frame = new FirstFrame(_context.Read(FirstFrame.GetPayload(_context.FrameLayerCapacity)), _context.PacketLength);
//                Connection.SendFrame(frame);
//                Connection.SetNextState(new WaitForControlFrameState(Connection, transaction));
//            }
//        }
//
//        public override IIsoTpState Operate(IsoTpFrame Frame)
//        {
//        }
//    }
}