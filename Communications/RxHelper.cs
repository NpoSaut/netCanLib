using System;
using System.Threading;

namespace Communications
{
    public static class RxHelper
    {
        public static TimeSpan RxInfinite(this TimeSpan Span)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return Span.TotalMilliseconds == Timeout.Infinite
                       ? TimeSpan.FromHours(1)
                       : Span;
        }
    }
}
