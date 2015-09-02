using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Threading;
using NLog;

namespace Communications.Appi.Timeouts
{
    public class SchedulerTimeoutManager<TTimeoutInformation> : ITimeoutManager<TTimeoutInformation>
    {
        private int _timerId = 0;
        private readonly ILogger _logger;
        private readonly IScheduler _scheduler;
        //private readonly Stopwatch _sw = new Stopwatch();
        private readonly Action<TTimeoutInformation> _timeoutAction;
        private IDisposable _timeoutToken;

        public SchedulerTimeoutManager(String Name, Action<TTimeoutInformation> TimeoutAction, IScheduler Scheduler)
        {
            _logger = LogManager.GetLogger(String.Format("{0} Timeouts", Name));
            _timeoutAction = TimeoutAction;
            _scheduler = Scheduler;
        }

        public void CockTimer(TimeSpan Timeout, TTimeoutInformation Information)
        {
            if (_timeoutToken != null)
                _timeoutToken.Dispose();

            int timerId = Interlocked.Increment(ref _timerId);
            //_sw.Restart();
            _timeoutToken = _scheduler.Schedule(Timeout, () =>
                                                         {
                                                             _logger.Error("Вышел таймаут {0}", timerId);
                                                             _timeoutAction(Information);
                                                         });
            _logger.Trace("Взводим таймер {0}", timerId);
        }

        public void DecockTimer()
        {
            if (_timeoutToken != null)
            {
                _timeoutToken.Dispose();
                _logger.Trace("Отменили таймер {0}", _timerId);
                //_logger.Trace("Отменили таймер {1} (на часах было {0})", _sw.ElapsedMilliseconds, _timerId);
                //_sw.Stop();
            }
        }
    }
}
