using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communications
{
    public interface ILog
    {
        void PushTextEvent(string EventName, string EventDetails = null);
    }

    public static class LogAgregatorHelper
    {
        public static void PushTextEvent(this IEnumerable<ILog> Logs, string EventName, string EventDetails = null)
        {
            if (Logs != null)
                foreach (var l in Logs) l.PushTextEvent(EventName, EventDetails);
        }
    }
}
