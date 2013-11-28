using System;
using System.Collections.Generic;
using System.IO;

namespace Communications.Can.LogRecording
{
    public abstract class LogRecorder : IDisposable
    {
        public FileInfo LogFile { get; set; }
        public CanPort Port { get; set; }
        public Stream FileStream { get; set; }

        protected LogRecorder(CanPort Port, FileInfo LogFile)
        {
            this.Port = Port;
            this.LogFile = LogFile;
            FileStream = LogFile.Open(FileMode.Append, FileAccess.Write);

            Port.Received += PortReceived;
        }

        private void PortReceived(object sender, CanFramesReceiveEventArgs e)
        {
            WriteFrames(e.Frames);
            FileStream.Flush();
        }

        public abstract void WriteFrames(IEnumerable<CanFrame> Frames);

        public virtual void Dispose()
        {
            Port.Received -= PortReceived;
            FileStream.Close();
        }
    }
}
