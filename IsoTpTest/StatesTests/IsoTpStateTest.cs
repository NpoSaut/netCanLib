using System.IO;
using Communications.Protocols.IsoTP;
using Communications.Protocols.IsoTP.States;

namespace IsoTpTest.StatesTests
{
    public abstract class IsoTpStateTest
    {
        protected class TestContext : ITransactionContext
        {
            public TestContext(int BlockSize) { this.BlockSize = BlockSize; }

            /// <summary>Размер одного блока</summary>
            public int BlockSize { get; private set; }

            /// <summary>Поток данных (входящих или исходящих)</summary>
            public MemoryStream DataStream { get; private set; }
            
            public void OnTransactionReady() { throw new System.NotImplementedException(); }
            public void SetNextState(IsoTpState NextState) { throw new System.NotImplementedException(); }
            public void SendControlFrame() { throw new System.NotImplementedException(); }
            public void CreateBuffer(int PacketSize) { throw new System.NotImplementedException(); }
            public void WriteToBuffer(byte[] Bytes) { throw new System.NotImplementedException(); }
        }
    }
}