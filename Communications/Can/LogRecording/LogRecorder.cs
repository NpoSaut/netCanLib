using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Communications.Can.LogRecording
{
    public abstract class LogRecorder : IDisposable
    {
        public FileInfo LogFile { get; set; }
        public Stream FileStream { get; set; }
        public ISocket<CanFrame> Socket { get; private set; }

        private readonly Thread writingThread;

        private void WriteJob()
        {
            while (true)
            {
                WriteFrames(Socket.Receive(TimeSpan.FromMilliseconds(100)));
                FileStream.Flush();
            }
        }

        protected LogRecorder(CanPort Port, FileInfo LogFile)
        {
            this.LogFile = LogFile;
            FileStream = LogFile.Open(FileMode.Append, FileAccess.Write);

            Socket = Port.OpenSocket();
            
            writingThread = new Thread(WriteJob) { IsBackground = true, Name = string.Format("Поток записи порта {0}", Port.Name), };

        }

        public abstract void WriteFrames(IEnumerable<CanFrame> Frames);

        public virtual void Dispose()
        {
            writingThread.Abort();
            Socket.Dispose();
            FileStream.Close();
        }
    }
}
