using System.Collections.Generic;

namespace Communications
{
    public interface ILog
    {
        void PushTextEvent(string EventName, string EventDetails = null);
    }

    public static class LogAggregatorHelper
    {
        public static void PushTextEvent(this IEnumerable<ILog> Logs, string EventName, string EventDetails = null)
        {
            if (Logs != null)
                foreach (var l in Logs) l.PushTextEvent(EventName, EventDetails);
        }
    }
}
