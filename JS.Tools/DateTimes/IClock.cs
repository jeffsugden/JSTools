using System;

namespace JS.Tools.DateTimes
{
    public interface IClock
    {
        DateTimeOffset UtcNow();
    }
}