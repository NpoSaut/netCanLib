using System;
using System.Collections.Generic;
using Communications.Can;

namespace Communications.Tests.Stuff
{
    public static class CanFrameHelper
    {
        public static IEnumerable<CanFrame> GetRandomFrames()
        {
            var r = new Random();
            while (true)
            {
                var data = new byte[r.Next(0, 8)];
                r.NextBytes(data);
                yield return CanFrame.NewWithId((UInt16)r.Next(0, 0x7ff), data);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
