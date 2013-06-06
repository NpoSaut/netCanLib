﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Communications.Can.LogRecording
{
    public abstract class LogRecorder : IDisposable
    {
        public FileInfo LogFile { get; set; }
        public CanPort Port { get; set; }

        public LogRecorder(CanPort Port, FileInfo LogFile)
        {
            this.Port = Port;
            this.LogFile = LogFile;

            Port.Recieved += Port_Recieved;
        }

        private void Port_Recieved(object sender, CanFramesReceiveEventArgs e)
        {
            WriteFrames(e.Frames);
        }

        public abstract void WriteFrames(IEnumerable<CanFrame> Frames);

        public void Dispose()
        {
            Port.Recieved -= Port_Recieved;
        }
    }
}
