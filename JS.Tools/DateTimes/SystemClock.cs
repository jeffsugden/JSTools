using System;

namespace JS.Tools.DateTimes
{
    internal class SystemClock : IClock
    {
        public DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;
    }
}