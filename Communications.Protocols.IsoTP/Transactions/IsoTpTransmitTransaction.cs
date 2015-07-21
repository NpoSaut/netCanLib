using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Communications.Protocols.IsoTP.Exceptions;
using Communications.Protocols.IsoTP.Frames;
using Communications.Protocols.IsoTP.ReceiveStates;

namespace Communications.Protocols.IsoTP.Transactions
{
    internal class TransmitState : IIsoTpState
    {
        private readonly IsoTpPacket _packet;
        private readonly IIsoTpFramesPort _port;
        private readonly IsoTpConnectionParameters _connectionParameters;
        
        private IBuffer<byte> _dataFlow;

        public void Activate()
        {
            _dataFlow = _packet.Data.Share();
            var firstFrame = new FirstFrame(_dataFlow.Take(FirstFrame.GetPayload(_port.Options.SublayerFrameCapacity)).ToArray(), _packet.Data.Length);
        }

        void SendFirstFrame()


        public TransmitState(IsoTpPacket Packet, IIsoTpFramesPort Port, IsoTpConnectionParameters ConnectionParameters)
        {
            _packet = Packet;
            _port = Port;
            _connectionParameters = ConnectionParameters;
        }

        private void SendLong(IsoTpPacket Packet, TimeSpan Timeout)
        {
            int[] sent = { 0 };
            IBuffer<byte> dataFlow = Packet.Data.Share();
            IEnumerable<ConsecutiveFrame> cfFlow = dataFlow.Buffer(ConsecutiveFrame.GetPayload(_port.Options.SublayerFrameCapacity))
                                                           .Select((d, i) => new ConsecutiveFrame(d.ToArray(), (i + 1) & 0x0f))
                                                           .Share();

            IObservable<ConsecutiveFrame> sendEngine = _port.Rx.Do(ValidateFrameType)
                                                            .Cast<FlowControlFrame>()
                                                            .Do(CheckForAbort)
                                                            .Timeout(Timeout)
                                                            .Where(fc => fc.Flag == FlowControlFlag.ClearToSend)
                                                            .SelectMany(fc => cfFlow.Take(fc.BlockSize)
                                                                                    .ToObservable()
                                                                                    .Do(cf => Thread.Sleep(fc.SeparationTime)) // TODO: Сделать задержку лучше
                                                                                    .Do(cf => sent[0] += cf.Data.Length));

            using (sendEngine.Subscribe(_port.Tx))
            {
                var firstFrame = new FirstFrame(dataFlow.Take(FirstFrame.GetPayload(_port.Options.SublayerFrameCapacity)).ToArray(), Packet.Data.Length);
                sent[0] += firstFrame.Data.Length;
                _port.Tx.OnNext(firstFrame);
                SpinWait.SpinUntil(() => sent[0] == Packet.Data.Length);
            }
        }

        private void ValidateFrameType(IsoTpFrame Frame)
        {
            if (!(Frame is FlowControlFrame))
                throw new IsoTpWrongFrameException(Frame, typeof (FlowControlFrame));
        }

        private void CheckForAbort(FlowControlFrame Frame)
        {
            if (Frame.Flag == FlowControlFlag.Abort)
                throw new IsoTpTransactionAbortedException();
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public void Dispose() { throw new NotImplementedException(); }

        public IIsoTpState Operate(IsoTpFrame Frame) { throw new NotImplementedException(); }
        public IIsoTpState OnException(Exception e) { throw new NotImplementedException(); }
        public void Abort() { throw new NotImplementedException(); }
        public event EventHandler<WannaSendEventArgs> WannaSend;
    }
}
