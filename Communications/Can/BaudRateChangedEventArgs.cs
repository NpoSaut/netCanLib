using System;

namespace Communications.Can
{
    public class BaudRateChangedEventArgs : EventArgs
    {
        public BaudRateChangedEventArgs(int NewValue) { NewBaudRate = NewValue; }
        public int NewBaudRate { get; private set; }
    }
}
