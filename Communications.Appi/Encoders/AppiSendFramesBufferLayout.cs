namespace Communications.Appi.Encoders
{
    public class AppiSendFramesBufferLayout
    {
        public AppiSendFramesBufferLayout(int InterfaceCodeOffset, int CommandCodeOffset, int FramesCountOffset, int FramesBodyOffset)
        {
            this.InterfaceCodeOffset = InterfaceCodeOffset;
            this.CommandCodeOffset = CommandCodeOffset;
            this.FramesCountOffset = FramesCountOffset;
            this.FramesBodyOffset = FramesBodyOffset;
        }

        public int InterfaceCodeOffset { get; private set; }
        public int CommandCodeOffset { get; private set; }
        public int FramesCountOffset { get; private set; }
        public int FramesBodyOffset { get; private set; }
    }
}
